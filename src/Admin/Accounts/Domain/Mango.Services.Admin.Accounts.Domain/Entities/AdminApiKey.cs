namespace Mango.Services.Admin.Accounts.Domain.Entities;

/// <summary>
/// Represents an API key for admin authentication. Keys are hashed and salted.
/// </summary>
public class AdminApiKey : BaseEntity
{
    /// <summary>
    /// ID of the admin user who owns this key.
    /// </summary>
    public int AdminUserId { get; set; }

    /// <summary>
    /// Navigation property to admin user.
    /// </summary>
    public AdminUser? AdminUser { get; set; }

    /// <summary>
    /// Bcrypt-hashed key. The plaintext key is never stored.
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// First 8 characters of the original key for display purposes.
    /// Allows users to identify which key is being used.
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Expiration date for the API key. Keys become invalid after this date.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Flag indicating if the key has been explicitly revoked.
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Timestamp when the key was revoked (if applicable).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Validates if the API key is currently valid for use.
    /// </summary>
    /// <returns>True if key is valid (not expired, not revoked, not deleted).</returns>
    public bool IsValid()
    {
        return !IsRevoked &&
               !IsExpired() &&
               ExpiresAt != default;
    }

    /// <summary>
    /// Checks if the API key has expired.
    /// </summary>
    /// <returns>True if the current time is past ExpiresAt.</returns>
    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// Validates a plaintext key against the stored hash.
    /// </summary>
    /// <param name="plainKey">The plaintext API key to validate.</param>
    /// <param name="hashingService">Service for key verification.</param>
    /// <returns>True if key matches and is valid.</returns>
    public bool ValidateKey(string plainKey, IApiKeyHashingService hashingService)
    {
        if (string.IsNullOrWhiteSpace(plainKey) || !IsValid())
            return false;

        return hashingService.VerifyKey(plainKey, KeyHash);
    }

    /// <summary>
    /// Revokes the API key.
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Interface for API key hashing service used during validation.
/// </summary>
public interface IApiKeyHashingService
{
    string HashKey(string plainKey);
    bool VerifyKey(string plainKey, string hash);
}
