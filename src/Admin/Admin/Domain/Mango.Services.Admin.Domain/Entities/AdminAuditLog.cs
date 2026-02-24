namespace Mango.Services.Admin.Domain.Entities;

/// <summary>
/// Audit log entry for tracking all admin operations.
/// Provides complete audit trail for compliance and security.
/// </summary>
public class AdminAuditLog : BaseEntity
{
    /// <summary>
    /// ID of the admin user who performed the action.
    /// </summary>
    public int AdminUserId { get; set; }

    /// <summary>
    /// Description of the action performed (e.g., "Create Product", "Update Order Status").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity affected (e.g., "Product", "Order", "Customer").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity that was affected (can be int or GUID as string).
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// JSON serialized before/after values for tracking changes.
    /// </summary>
    public string? Changes { get; set; }

    /// <summary>
    /// IP address of the request that triggered this action.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Validates the audit log entry has required data.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Action) &&
               !string.IsNullOrWhiteSpace(EntityType) &&
               !string.IsNullOrWhiteSpace(EntityId) &&
               AdminUserId > 0;
    }
}
