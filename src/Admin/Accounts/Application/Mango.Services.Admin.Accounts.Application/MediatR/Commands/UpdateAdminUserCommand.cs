using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Application.Interfaces;
using Mango.Services.Admin.Accounts.Domain.Entities;

namespace Mango.Services.Admin.Accounts.Application.MediatR.Commands;

/// <summary>
/// Command to update an admin user.
/// </summary>
public class UpdateAdminUserCommand : BaseCommand<AdminUserDto>
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public AdminRole Role { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Handler for UpdateAdminUserCommand.
/// </summary>
public class UpdateAdminUserCommandHandler : IRequestHandler<UpdateAdminUserCommand, ResponseDto<AdminUserDto>>
{
    private readonly IAdminUserRepository _userRepository;

    public UpdateAdminUserCommandHandler(IAdminUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<ResponseDto<AdminUserDto>> Handle(UpdateAdminUserCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (request.Id <= 0)
        {
            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = false,
                Message = "Invalid admin user ID"
            };
        }

        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length < 2 || request.FullName.Length > 100)
        {
            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = false,
                Message = "Full name must be between 2 and 100 characters"
            };
        }

        // Get the existing user
        var adminUser = await _userRepository.GetByIdAsync(request.Id);
        if (adminUser == null)
        {
            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = false,
                Message = "Admin user not found"
            };
        }

        try
        {
            // Update user properties
            adminUser.FullName = request.FullName;
            adminUser.Role = request.Role;
            adminUser.IsActive = request.IsActive;
            adminUser.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(adminUser);
            await _userRepository.SaveChangesAsync();

            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = true,
                Message = "Admin user updated successfully",
                Result = new AdminUserDto
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
            };
        }
        catch (Exception ex)
        {
            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = false,
                Message = $"Error updating admin user: {ex.Message}"
            };
        }
    }
}
