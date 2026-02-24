using Mango.Services.Admin.Accounts.Domain.Entities;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Mango.Services.Admin.Accounts.Infrastructure.Services;

/// <summary>
/// Service for securely hashing and verifying API keys using BCrypt.
/// </summary>
public class ApiKeyHashingService : IApiKeyHashingService
{
    /// <summary>
    /// Hashes a plaintext API key using BCrypt with 12 salt rounds.
    /// </summary>
    /// <param name="plainKey">The plaintext API key to hash.</param>
    /// <returns>The BCrypt-hashed key.</returns>
    public string HashKey(string plainKey)
    {
        if (string.IsNullOrWhiteSpace(plainKey))
            throw new ArgumentException("API key cannot be empty", nameof(plainKey));

        return BCryptNet.HashPassword(plainKey, workFactor: 12);
    }

    /// <summary>
    /// Verifies a plaintext API key against a stored BCrypt hash.
    /// </summary>
    /// <param name="plainKey">The plaintext API key to verify.</param>
    /// <param name="hash">The stored BCrypt hash.</param>
    /// <returns>True if the key matches the hash, false otherwise.</returns>
    public bool VerifyKey(string plainKey, string hash)
    {
        if (string.IsNullOrWhiteSpace(plainKey) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCryptNet.Verify(plainKey, hash);
        }
        catch (Exception)
        {
            // Catch all BCrypt exceptions (InvalidOperationException, SaltParseException, etc.)
            return false;
        }
    }
}
