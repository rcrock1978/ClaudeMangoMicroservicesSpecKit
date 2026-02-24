using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Application.Interfaces;
using Mango.Services.Admin.Accounts.Domain.Entities;

namespace Mango.Services.Admin.Accounts.Application.MediatR.Queries;

/// <summary>
/// Query to validate an API key and retrieve admin user information.
/// </summary>
public class ValidateApiKeyQuery : BaseQuery<ValidateApiKeyResponse>
{
    public string ApiKey { get; set; }

    public ValidateApiKeyQuery(string apiKey)
    {
        ApiKey = apiKey;
    }
}

/// <summary>
/// Handler for ValidateApiKeyQuery.
/// </summary>
public class ValidateApiKeyQueryHandler : IRequestHandler<ValidateApiKeyQuery, ResponseDto<ValidateApiKeyResponse>>
{
    private readonly IAdminApiKeyRepository _apiKeyRepository;
    private readonly IAdminUserRepository _userRepository;
    private readonly IApiKeyHashingService _hashingService;

    public ValidateApiKeyQueryHandler(
        IAdminApiKeyRepository apiKeyRepository,
        IAdminUserRepository userRepository,
        IApiKeyHashingService hashingService)
    {
        _apiKeyRepository = apiKeyRepository;
        _userRepository = userRepository;
        _hashingService = hashingService;
    }

    public async Task<ResponseDto<ValidateApiKeyResponse>> Handle(ValidateApiKeyQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            return new ResponseDto<ValidateApiKeyResponse>
            {
                IsSuccess = false,
                Message = "API key is required"
            };
        }

        // Extract key prefix for lookup
        string keyPrefix = request.ApiKey[..Math.Min(8, request.ApiKey.Length)];
        var apiKey = await _apiKeyRepository.GetByKeyPrefixAsync(keyPrefix);

        if (apiKey == null)
        {
            return new ResponseDto<ValidateApiKeyResponse>
            {
                IsSuccess = false,
                Message = "Invalid API key"
            };
        }

        // Validate the key
        if (!apiKey.ValidateKey(request.ApiKey, _hashingService))
        {
            return new ResponseDto<ValidateApiKeyResponse>
            {
                IsSuccess = false,
                Message = "Invalid API key"
            };
        }

        // Get the admin user
        var adminUser = await _userRepository.GetByIdAsync(apiKey.AdminUserId);
        if (adminUser == null || !adminUser.IsValid())
        {
            return new ResponseDto<ValidateApiKeyResponse>
            {
                IsSuccess = false,
                Message = "Admin user is inactive or not found"
            };
        }

        // Record login
        adminUser.RecordLogin();
        await _userRepository.UpdateAsync(adminUser);
        await _userRepository.SaveChangesAsync();

        return new ResponseDto<ValidateApiKeyResponse>
        {
            IsSuccess = true,
            Message = "API key validation successful",
            Result = new ValidateApiKeyResponse
            {
                IsValid = true,
                AdminUserId = adminUser.Id,
                AdminUser = new AdminUserDto
                {
                    Id = adminUser.Id,
                    Email = adminUser.Email,
                    FullName = adminUser.FullName,
                    Role = adminUser.Role,
                    IsActive = adminUser.IsActive,
                    LastLoginAt = adminUser.LastLoginAt,
                    CreatedAt = adminUser.CreatedAt,
                    UpdatedAt = adminUser.UpdatedAt
                }
            }
        };
    }
}
