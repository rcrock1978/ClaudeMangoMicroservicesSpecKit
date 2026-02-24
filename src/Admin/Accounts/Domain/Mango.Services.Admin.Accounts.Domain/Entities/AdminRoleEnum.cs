namespace Mango.Services.Admin.Accounts.Domain.Entities;

/// <summary>
/// Admin role enumeration for authorization levels.
/// </summary>
public enum AdminRole
{
    /// <summary>
    /// Full system access - can manage all admin users and all operations.
    /// </summary>
    SUPER_ADMIN = 1,

    /// <summary>
    /// Can manage orders, customers, reports, and view dashboards.
    /// </summary>
    ADMIN = 2,

    /// <summary>
    /// Read-only access to dashboards and reports.
    /// </summary>
    MODERATOR = 3
}
