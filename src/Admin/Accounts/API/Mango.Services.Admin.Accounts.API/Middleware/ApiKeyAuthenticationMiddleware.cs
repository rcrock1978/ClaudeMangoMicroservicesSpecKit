using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Application.Interfaces;
using Mango.Services.Admin.Accounts.Application.MediatR.Queries;
using System.Security.Claims;

namespace Mango.Services.Admin.Accounts.API.Middleware;

/// <summary>
/// Middleware for API key-based authentication.
/// Extracts API key from X-API-Key header and validates it.
/// </summary>
public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMediator mediator)
    {
        var endpoint = context.GetEndpoint();

        // Skip authentication for health checks and public endpoints
        if (endpoint?.Metadata.GetMetadata<SkipApiKeyAuthAttribute>() != null)
        {
            await _next(context);
            return;
        }

        // Check for API key in header
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyValue))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ResponseDto
            {
                IsSuccess = false,
                Message = $"API key is required in {ApiKeyHeaderName} header"
            });
            return;
        }

        // Validate the API key
        var query = new ValidateApiKeyQuery(apiKeyValue.ToString());
        var result = await mediator.Send(query);

        if (!result.IsSuccess || result.Result?.IsValid != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ResponseDto
            {
                IsSuccess = false,
                Message = result.Message ?? "Invalid API key"
            });
            return;
        }

        // Set user claims in context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.Result.AdminUserId.ToString()),
            new Claim(ClaimTypes.Email, result.Result.AdminUser?.Email ?? ""),
            new Claim(ClaimTypes.Name, result.Result.AdminUser?.FullName ?? ""),
            new Claim("AdminRole", result.Result.AdminUser?.Role.ToString() ?? ""),
            new Claim("AdminId", result.Result.AdminUserId.ToString())
        };

        var identity = new ClaimsIdentity(claims, ApiKeyHeaderName);
        var principal = new ClaimsPrincipal(identity);
        context.User = principal;

        await _next(context);
    }
}

/// <summary>
/// Attribute to skip API key authentication for specific endpoints.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipApiKeyAuthAttribute : Attribute
{
}
