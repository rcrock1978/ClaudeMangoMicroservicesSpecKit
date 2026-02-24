using Mango.Services.Admin.Accounts.Domain.Entities;

namespace Mango.Services.Admin.Accounts.Application.DTOs;

/// <summary>
/// Data transfer object for admin user information.
/// </summary>
public class AdminUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public AdminRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request model for creating a new admin user.
/// </summary>
public class CreateAdminUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public AdminRole Role { get; set; } = AdminRole.MODERATOR;
}

/// <summary>
/// Request model for updating an admin user.
/// </summary>
public class UpdateAdminUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public AdminRole Role { get; set; }
    public bool IsActive { get; set; }
}
