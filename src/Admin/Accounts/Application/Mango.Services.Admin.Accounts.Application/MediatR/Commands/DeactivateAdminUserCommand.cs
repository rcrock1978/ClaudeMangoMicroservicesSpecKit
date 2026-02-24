using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Application.Interfaces;

namespace Mango.Services.Admin.Accounts.Application.MediatR.Commands;

/// <summary>
/// Command to deactivate an admin user (soft delete).
/// </summary>
public class DeactivateAdminUserCommand : BaseCommand<bool>
{
    public int Id { get; set; }
}

/// <summary>
/// Handler for DeactivateAdminUserCommand.
/// </summary>
public class DeactivateAdminUserCommandHandler : IRequestHandler<DeactivateAdminUserCommand, ResponseDto<bool>>
{
    private readonly IAdminUserRepository _userRepository;

    public DeactivateAdminUserCommandHandler(IAdminUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<ResponseDto<bool>> Handle(DeactivateAdminUserCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (request.Id <= 0)
        {
            return new ResponseDto<bool>
            {
                IsSuccess = false,
                Message = "Invalid admin user ID"
            };
        }

        // Get the existing user
        var adminUser = await _userRepository.GetByIdAsync(request.Id);
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
            // Soft delete: mark as inactive
            adminUser.IsActive = false;
            adminUser.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(adminUser);
            await _userRepository.SaveChangesAsync();

            return new ResponseDto<bool>
            {
                IsSuccess = true,
                Message = "Admin user deactivated successfully",
                Result = true
            };
        }
        catch (Exception ex)
        {
            return new ResponseDto<bool>
            {
                IsSuccess = false,
                Message = $"Error deactivating admin user: {ex.Message}"
            };
        }
    }
}
