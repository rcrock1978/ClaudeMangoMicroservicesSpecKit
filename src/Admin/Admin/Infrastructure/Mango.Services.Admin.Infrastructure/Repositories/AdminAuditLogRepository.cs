using Microsoft.EntityFrameworkCore;
using Mango.Services.Admin.Domain.Entities;
using Mango.Services.Admin.Application.Interfaces;
using Mango.Services.Admin.Infrastructure.Data;

namespace Mango.Services.Admin.Infrastructure.Repositories;

/// <summary>
/// Repository for persisting and retrieving admin audit log entries.
/// </summary>
public class AdminAuditLogRepository : IAdminAuditLogRepository
{
    private readonly AdminDbContext _context;

    public AdminAuditLogRepository(AdminDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Logs an admin action to the audit trail.
    /// </summary>
    /// <param name="adminUserId">ID of the admin performing the action.</param>
    /// <param name="action">Description of the action (e.g., "Create Product").</param>
    /// <param name="entityType">Type of entity affected (e.g., "Product").</param>
    /// <param name="entityId">ID of the affected entity.</param>
    /// <param name="changes">JSON representation of the changes made.</param>
    /// <param name="ipAddress">IP address of the request source.</param>
    /// <returns>The created audit log entry.</returns>
    public async Task<AdminAuditLog> LogActionAsync(
        int adminUserId,
        string action,
        string entityType,
        string entityId,
        string? changes = null,
        string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty", nameof(action));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType cannot be empty", nameof(entityType));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("EntityId cannot be empty", nameof(entityId));

        var auditLog = new AdminAuditLog
        {
            AdminUserId = adminUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Changes = changes,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        if (!auditLog.IsValid())
            throw new InvalidOperationException("Audit log validation failed");

        _context.AdminAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return auditLog;
    }

    /// <summary>
    /// Retrieves paginated audit log entries with optional filtering.
    /// </summary>
    /// <param name="adminUserId">Optional filter by admin user ID.</param>
    /// <param name="entityType">Optional filter by entity type.</param>
    /// <param name="startDate">Optional filter for logs created after this date.</param>
    /// <param name="endDate">Optional filter for logs created before this date.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <returns>List of audit log entries matching criteria, ordered by creation date descending.</returns>
    public async Task<List<AdminAuditLog>> GetLogsAsync(
        int? adminUserId = null,
        string? entityType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1 || pageSize > 100)
            pageSize = 20;

        var query = _context.AdminAuditLogs.AsQueryable();

        // Apply filters
        if (adminUserId.HasValue)
            query = query.Where(x => x.AdminUserId == adminUserId.Value);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(x => x.EntityType == entityType);

        if (startDate.HasValue)
            query = query.Where(x => x.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.CreatedAt <= endDate.Value);

        // Order by most recent first
        query = query.OrderByDescending(x => x.CreatedAt);

        // Apply pagination
        var skip = (pageNumber - 1) * pageSize;
        return await query.Skip(skip).Take(pageSize).ToListAsync();
    }

    /// <summary>
    /// Gets the count of audit log entries matching optional criteria.
    /// </summary>
    /// <param name="adminUserId">Optional filter by admin user ID.</param>
    /// <param name="entityType">Optional filter by entity type.</param>
    /// <param name="startDate">Optional filter for logs created after this date.</param>
    /// <param name="endDate">Optional filter for logs created before this date.</param>
    /// <returns>Total count of matching audit log entries.</returns>
    public async Task<int> GetLogsCountAsync(
        int? adminUserId = null,
        string? entityType = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _context.AdminAuditLogs.AsQueryable();

        if (adminUserId.HasValue)
            query = query.Where(x => x.AdminUserId == adminUserId.Value);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(x => x.EntityType == entityType);

        if (startDate.HasValue)
            query = query.Where(x => x.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.CreatedAt <= endDate.Value);

        return await query.CountAsync();
    }

    /// <summary>
    /// Retrieves the complete history of changes for a specific entity.
    /// </summary>
    /// <param name="entityType">Type of entity to retrieve history for.</param>
    /// <param name="entityId">ID of the entity.</param>
    /// <returns>List of all audit log entries for the entity, ordered chronologically.</returns>
    public async Task<List<AdminAuditLog>> GetEntityHistoryAsync(string entityType, string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType cannot be empty", nameof(entityType));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("EntityId cannot be empty", nameof(entityId));

        return await _context.AdminAuditLogs
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the most recent action for a specific entity.
    /// </summary>
    /// <param name="entityType">Type of entity.</param>
    /// <param name="entityId">ID of the entity.</param>
    /// <returns>The most recent audit log entry for the entity, or null if none exist.</returns>
    public async Task<AdminAuditLog?> GetLatestEntityAuditAsync(string entityType, string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType cannot be empty", nameof(entityType));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("EntityId cannot be empty", nameof(entityId));

        return await _context.AdminAuditLogs
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets actions performed by a specific admin user.
    /// </summary>
    /// <param name="adminUserId">ID of the admin user.</param>
    /// <param name="startDate">Optional filter for logs created after this date.</param>
    /// <param name="endDate">Optional filter for logs created before this date.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <returns>Paginated list of audit logs for the admin user.</returns>
    public async Task<List<AdminAuditLog>> GetAdminActionsAsync(
        int adminUserId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        return await GetLogsAsync(
            adminUserId: adminUserId,
            startDate: startDate,
            endDate: endDate,
            pageNumber: pageNumber,
            pageSize: pageSize);
    }
}
