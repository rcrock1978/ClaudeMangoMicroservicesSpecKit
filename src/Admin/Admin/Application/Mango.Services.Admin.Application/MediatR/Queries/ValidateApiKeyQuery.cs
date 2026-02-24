using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http.Json;

namespace Mango.Services.Admin.Application.MediatR.Queries;

/// <summary>
/// Query to validate an API key and retrieve associated admin information.
/// Used by authentication middleware to verify API key validity.
/// </summary>
public class ValidateApiKeyQuery : IRequest<ValidateApiKeyResult>
{
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Result of API key validation containing admin user information if valid.
/// </summary>
public class ValidateApiKeyResult
{
    public bool IsValid { get; set; }
    public int AdminUserId { get; set; }
    public AdminUserInfo AdminUser { get; set; } = new();
}

/// <summary>
/// Admin user information returned from API key validation.
/// </summary>
public class AdminUserInfo
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Handler for ValidateApiKeyQuery.
/// Validates API key against stored credentials via HTTP call to Admin.Accounts service.
/// </summary>
public class ValidateApiKeyQueryHandler : IRequestHandler<ValidateApiKeyQuery, ValidateApiKeyResult>
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ValidateApiKeyQueryHandler> _logger;

    public ValidateApiKeyQueryHandler(HttpClient httpClient, ILogger<ValidateApiKeyQueryHandler> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ValidateApiKeyResult> Handle(ValidateApiKeyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ApiKey))
            {
                _logger.LogWarning("ValidateApiKeyQuery received empty API key");
                return new ValidateApiKeyResult { IsValid = false };
            }

            // Call Admin.Accounts service to validate API key
            var response = await _httpClient.PostAsJsonAsync(
                "http://mango-admin-accounts-api:8080/api/admin/accounts/validate-key",
                new { ApiKey = request.ApiKey },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API key validation failed with status code {StatusCode}", response.StatusCode);
                return new ValidateApiKeyResult { IsValid = false };
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<ValidateApiKeyResult>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result ?? new ValidateApiKeyResult { IsValid = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API key");
            return new ValidateApiKeyResult { IsValid = false };
        }
    }
}
