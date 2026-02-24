using Mango.Services.Email.Application.Interfaces;
using Mango.Services.Email.Domain.Entities;
using Mango.Services.Email.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.Email.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for email data access operations.
/// </summary>
public class EmailRepository : IEmailRepository
{
    private readonly EmailDbContext _context;

    public EmailRepository(EmailDbContext context)
    {
        _context = context;
    }

    // Email Log operations
    public async Task AddEmailLogAsync(EmailLog emailLog, CancellationToken cancellationToken = default)
    {
        _context.EmailLogs.Add(emailLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateEmailLogAsync(EmailLog emailLog, CancellationToken cancellationToken = default)
    {
        _context.EmailLogs.Update(emailLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmailLog?> GetEmailLogAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<List<EmailLog>> GetEmailLogsAsync(string? userId = null, int? orderId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.EmailLogs.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(l => l.UserId == userId);
        }

        if (orderId.HasValue)
        {
            query = query.Where(l => l.OrderId == orderId);
        }

        return await query.OrderByDescending(l => l.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<List<EmailLog>> GetFailedEmailLogsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs
            .Where(l => !l.IsSent && l.AttemptCount < 3)
            .OrderBy(l => l.LastAttemptAt)
            .ToListAsync(cancellationToken);
    }

    // Email Template operations
    public async Task<EmailTemplate> AddTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        _context.EmailTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task<EmailTemplate?> GetTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<EmailTemplate?> GetTemplateByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Name == name && t.IsActive, cancellationToken);
    }

    public async Task<List<EmailTemplate>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(id, cancellationToken);
        if (template == null)
        {
            return false;
        }

        template.IsDeleted = true;
        template.DeletedAt = DateTime.UtcNow;
        await UpdateTemplateAsync(template, cancellationToken);
        return true;
    }
}
