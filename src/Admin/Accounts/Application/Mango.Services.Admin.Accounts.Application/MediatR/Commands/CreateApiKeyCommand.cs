using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Application.Interfaces;
using Mango.Services.Admin.Accounts.Domain.Entities;

namespace Mango.Services.Admin.Accounts.Application.MediatR.Commands;

/// <summary>
/// Command to create a new API key for an admin user.
/// This will revoke any existing key for the user.
/// </summary>
public class CreateApiKeyCommand : BaseCommand<CreateApiKeyResponse>
{
    public int AdminUserId { get; set; }
}

/// <summary>
/// Handler for CreateApiKeyCommand.
/// </summary>
public class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, ResponseDto<CreateApiKeyResponse>>
{
    private readonly IAdminUserRepository _userRepository;
    private readonly IAdminApiKeyRepository _apiKeyRepository;
    private readonly IApiKeyHashingService _hashingService;

    public CreateApiKeyCommandHandler(
        IAdminUserRepository userRepository,
        IAdminApiKeyRepository apiKeyRepository,
        IApiKeyHashingService hashingService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
        _hashingService = hashingService ?? throw new ArgumentNullException(nameof(hashingService));
    }

    public async Task<ResponseDto<CreateApiKeyResponse>> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (request.AdminUserId <= 0)
        {
            return new ResponseDto<CreateApiKeyResponse>
            {
                IsSuccess = false,
                Message = "Invalid admin user ID"
            };
        }

        // Get the admin user
        var adminUser = await _userRepository.GetByIdAsync(request.AdminUserId);
        if (adminUser == null || !adminUser.IsActive)
        {
            return new ResponseDto<CreateApiKeyResponse>
            {
                IsSuccess = false,
                Message = "Admin user not found or is inactive"
            };
        }

        try
        {
            // Revoke existing API keys for this user
            var existingKey = await _apiKeyRepository.GetByAdminIdAsync(request.AdminUserId);
            if (existingKey != null)
            {
                existingKey.IsRevoked = true;
                existingKey.UpdatedAt = DateTime.UtcNow;
                await _apiKeyRepository.UpdateAsync(existingKey);
            }

            // Generate new API key
            var plainTextKey = GenerateApiKey();
            var keyHash = _hashingService.HashKey(plainTextKey);
            var keyPrefix = plainTextKey[..Math.Min(8, plainTextKey.Length)];
            var expiresAt = DateTime.UtcNow.AddYears(1);

            var newApiKey = new AdminApiKey
            {
                AdminUserId = request.AdminUserId,
                KeyHash = keyHash,
                KeyPrefix = keyPrefix,
                ExpiresAt = expiresAt,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _apiKeyRepository.AddAsync(newApiKey);
            await _apiKeyRepository.SaveChangesAsync();

            return new ResponseDto<CreateApiKeyResponse>
            {
                IsSuccess = true,
                Message = "API key created successfully. Save this key securely - it won't be shown again.",
                Result = new CreateApiKeyResponse
                {
                    AdminUserId = request.AdminUserId,
                    ApiKey = plainTextKey,
                    KeyPrefix = keyPrefix,
                    ExpiresAt = expiresAt
                }
            };
        }
        catch (Exception ex)
        {
            return new ResponseDto<CreateApiKeyResponse>
            {
                IsSuccess = false,
                Message = $"Error creating API key: {ex.Message}"
            };
        }
    }

    private static string GenerateApiKey()
    {
        // Generate a cryptographically secure random API key (32 bytes = 256 bits)
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = new char[32];
        var buffer = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }
        for (int i = 0; i < 32; i++)
        {
            result[i] = chars[buffer[i] % chars.Length];
        }
        return new string(result);
    }
}
