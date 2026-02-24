using MediatR;
using Mango.Services.Admin.Accounts.Application.DTOs;
using Mango.Services.Admin.Accounts.Application.Interfaces;
using Mango.Services.Admin.Accounts.Domain.Entities;

namespace Mango.Services.Admin.Accounts.Application.MediatR.Commands;

/// <summary>
/// Command to create a new admin user.
/// </summary>
public class CreateAdminUserCommand : BaseCommand<AdminUserDto>
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public AdminRole Role { get; set; } = AdminRole.MODERATOR;
}

/// <summary>
/// Handler for CreateAdminUserCommand.
/// </summary>
public class CreateAdminUserCommandHandler : IRequestHandler<CreateAdminUserCommand, ResponseDto<AdminUserDto>>
{
    private readonly IAdminUserRepository _userRepository;

    public CreateAdminUserCommandHandler(IAdminUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<ResponseDto<AdminUserDto>> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = false,
                Message = "Email is required"
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

        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = false,
                Message = "An admin user with this email already exists"
            };
        }

        // Create new admin user
        var adminUser = new AdminUser
        {
            Email = request.Email.ToLower(),
            FullName = request.FullName,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            await _userRepository.AddAsync(adminUser);
            await _userRepository.SaveChangesAsync();

            return new ResponseDto<AdminUserDto>
            {
                IsSuccess = true,
                Message = "Admin user created successfully",
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
                Message = $"Error creating admin user: {ex.Message}"
            };
        }
    }
}
