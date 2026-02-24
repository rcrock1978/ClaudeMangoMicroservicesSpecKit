namespace Mango.Services.Admin.Accounts.Domain.Entities;

/// <summary>
/// Represents an admin user in the system with role-based access control.
/// </summary>
public class AdminUser : BaseEntity
{
    /// <summary>
    /// Unique email address for the admin user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the admin user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Admin role determining authorization level.
    /// </summary>
    public AdminRole Role { get; set; } = AdminRole.MODERATOR;

    /// <summary>
    /// Is the admin account active and usable.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp of the last successful login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// API key associated with this admin user.
    /// </summary>
    public AdminApiKey? ApiKey { get; set; }

    /// <summary>
    /// Validates if the admin user is in a valid state for access.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(FullName) &&
               FullName.Length >= 2 &&
               FullName.Length <= 100 &&
               IsActive;
    }

    /// <summary>
    /// Checks if admin can access a specific resource based on role.
    /// </summary>
    /// <param name="requiredRole">Minimum required role to access resource.</param>
    /// <returns>True if admin has sufficient permission.</returns>
    public bool CanAccess(AdminRole requiredRole)
    {
        if (!IsActive)
            return false;

        // SUPER_ADMIN can access everything
        if (Role == AdminRole.SUPER_ADMIN)
            return true;

        // ADMIN can access everything except SUPER_ADMIN resources
        if (Role == AdminRole.ADMIN && requiredRole != AdminRole.SUPER_ADMIN)
            return true;

        // MODERATOR can only access MODERATOR resources
        return Role == requiredRole;
    }

    /// <summary>
    /// Records the login timestamp.
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}
