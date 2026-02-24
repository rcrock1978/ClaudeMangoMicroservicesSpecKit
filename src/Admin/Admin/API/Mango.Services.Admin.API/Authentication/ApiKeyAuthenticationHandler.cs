using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediatR;
using Mango.Services.Admin.Application.MediatR.Queries;

namespace Mango.Services.Admin.API.Authentication;

/// <summary>
/// Authentication handler for API key validation.
/// Extracts API key from X-API-Key header and validates it via MediatR query.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ApiKeyHeader = "X-API-Key";
    private readonly IMediator _mediator;
    private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IMediator mediator,
        ILogger<ApiKeyAuthenticationHandler> logger)
        : base(options, loggerFactory, encoder)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the authentication request by validating the API key.
    /// </summary>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if API key header exists
        if (!Request.Headers.TryGetValue(ApiKeyHeader, out var apiKeyValue))
        {
            _logger.LogInformation("API key header missing from request to {Path}", Request.Path);
            return AuthenticateResult.Fail("API key header is missing");
        }

        var apiKey = apiKeyValue.ToString();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogInformation("Empty API key provided for request to {Path}", Request.Path);
            return AuthenticateResult.Fail("API key cannot be empty");
        }

        try
        {
            // Validate API key via MediatR query
            var query = new ValidateApiKeyQuery { ApiKey = apiKey };
            var result = await _mediator.Send(query);

            if (result == null || !result.IsValid)
            {
                _logger.LogWarning("Invalid API key provided for request to {Path}", Request.Path);
                return AuthenticateResult.Fail("Invalid or expired API key");
            }

            // Create claims for authenticated admin user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.AdminUserId.ToString()),
                new Claim(ClaimTypes.Email, result.AdminUser.Email),
                new Claim(ClaimTypes.Name, result.AdminUser.FullName),
                new Claim("AdminRole", result.AdminUser.Role.ToString()),
                new Claim("AdminId", result.AdminUserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogInformation(
                "API key validated for admin user {AdminId} ({Email}) with role {Role}",
                result.AdminUserId,
                result.AdminUser.Email,
                result.AdminUser.Role);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API key");
            return AuthenticateResult.Fail("Error validating API key");
        }
    }

    /// <summary>
    /// Handles challenge response when authentication fails.
    /// </summary>
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";

        var response = new
        {
            error = "Unauthorized",
            message = "Valid API key is required. Provide it via X-API-Key header."
        };

        await Response.WriteAsJsonAsync(response);
    }

    /// <summary>
    /// Handles forbidden response when user lacks required permissions.
    /// </summary>
    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        Response.ContentType = "application/json";

        var response = new
        {
            error = "Forbidden",
            message = "Your API key does not have permission to access this resource."
        };

        await Response.WriteAsJsonAsync(response);
    }
}
