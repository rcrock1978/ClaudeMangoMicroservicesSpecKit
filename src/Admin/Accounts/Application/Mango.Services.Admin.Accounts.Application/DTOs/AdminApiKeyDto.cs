namespace Mango.Services.Admin.Accounts.Application.DTOs;

/// <summary>
/// Data transfer object for API key information (does not expose the key hash).
/// </summary>
public class AdminApiKeyDto
{
    public int Id { get; set; }
    public int AdminUserId { get; set; }
    public string KeyPrefix { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request model for creating a new API key.
/// </summary>
public class CreateApiKeyRequest
{
    public int AdminUserId { get; set; }
    /// <summary>
    /// Number of days until the key expires. Default: 365 days.
    /// </summary>
    public int ExpirationDays { get; set; } = 365;
}

/// <summary>
/// Response model for API key creation (contains the plaintext key one-time only).
/// </summary>
public class CreateApiKeyResponse
{
    public int AdminUserId { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Message { get; set; } = "Store this API key securely. You will not be able to see it again.";
}

/// <summary>
/// Request model for API key validation.
/// </summary>
public class ValidateApiKeyRequest
{
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Response model for API key validation.
/// </summary>
public class ValidateApiKeyResponse
{
    public bool IsValid { get; set; }
    public int? AdminUserId { get; set; }
    public AdminUserDto? AdminUser { get; set; }
    public string? ErrorMessage { get; set; }
}
