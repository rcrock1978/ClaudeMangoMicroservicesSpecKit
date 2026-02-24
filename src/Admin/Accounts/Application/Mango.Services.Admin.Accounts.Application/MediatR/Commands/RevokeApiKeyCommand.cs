using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Application.Interfaces;

namespace Mango.Services.Admin.Accounts.Application.MediatR.Commands;

/// <summary>
/// Command to revoke an API key for an admin user.
/// </summary>
public class RevokeApiKeyCommand : BaseCommand<bool>
{
    public int AdminUserId { get; set; }
}

/// <summary>
/// Handler for RevokeApiKeyCommand.
/// </summary>
public class RevokeApiKeyCommandHandler : IRequestHandler<RevokeApiKeyCommand, ResponseDto<bool>>
{
    private readonly IAdminUserRepository _userRepository;
    private readonly IAdminApiKeyRepository _apiKeyRepository;

    public RevokeApiKeyCommandHandler(
        IAdminUserRepository userRepository,
        IAdminApiKeyRepository apiKeyRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
    }

    public async Task<ResponseDto<bool>> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (request.AdminUserId <= 0)
        {
            return new ResponseDto<bool>
            {
                IsSuccess = false,
                Message = "Invalid admin user ID"
            };
        }

        // Get the admin user
        var adminUser = await _userRepository.GetByIdAsync(request.AdminUserId);
        if (adminUser == null)
        {
            return new ResponseDto<bool>
            {
                IsSuccess = false,
                Message = "Admin user not found"
            };
        }

        try
        {
            // Get and revoke the API key
            var apiKey = await _apiKeyRepository.GetByAdminIdAsync(request.AdminUserId);
            if (apiKey == null || apiKey.IsRevoked)
            {
                return new ResponseDto<bool>
                {
                    IsSuccess = false,
                    Message = "No active API key found for this user"
                };
            }

            apiKey.IsRevoked = true;
            apiKey.UpdatedAt = DateTime.UtcNow;

            await _apiKeyRepository.UpdateAsync(apiKey);
            await _apiKeyRepository.SaveChangesAsync();

            return new ResponseDto<bool>
            {
                IsSuccess = true,
                Message = "API key revoked successfully",
                Result = true
            };
        }
        catch (Exception ex)
        {
            return new ResponseDto<bool>
            {
                IsSuccess = false,
                Message = $"Error revoking API key: {ex.Message}"
            };
        }
    }
}
