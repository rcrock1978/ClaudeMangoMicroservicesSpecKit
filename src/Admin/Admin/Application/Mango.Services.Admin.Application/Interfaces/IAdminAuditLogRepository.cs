using Mango.Services.Admin.Domain.Entities;

namespace Mango.Services.Admin.Application.Interfaces;

/// <summary>
/// Repository interface for managing admin audit logs.
/// </summary>
public interface IAdminAuditLogRepository
{
    /// <summary>
    /// Logs an admin action to the audit trail.
    /// </summary>
    Task<AdminAuditLog> LogActionAsync(
        int adminUserId,
        string action,
        string entityType,
        string entityId,
        string? changes = null,
        string? ipAddress = null);

    /// <summary>
    /// Retrieves paginated audit log entries with optional filtering.
    /// </summary>
    Task<List<AdminAuditLog>> GetLogsAsync(
        int? adminUserId = null,
        string? entityType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 20);

    /// <summary>
    /// Gets the count of audit log entries matching optional criteria.
    /// </summary>
    Task<int> GetLogsCountAsync(
        int? adminUserId = null,
        string? entityType = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Retrieves the complete history of changes for a specific entity.
    /// </summary>
    Task<List<AdminAuditLog>> GetEntityHistoryAsync(string entityType, string entityId);

    /// <summary>
    /// Retrieves the most recent action for a specific entity.
    /// </summary>
    Task<AdminAuditLog?> GetLatestEntityAuditAsync(string entityType, string entityId);

    /// <summary>
    /// Gets actions performed by a specific admin user.
    /// </summary>
    Task<List<AdminAuditLog>> GetAdminActionsAsync(
        int adminUserId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 20);
}
