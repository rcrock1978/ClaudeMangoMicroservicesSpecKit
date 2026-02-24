using Mango.Services.Email.Domain.Entities;

namespace Mango.Services.Email.Application.Interfaces;

/// <summary>
/// Repository interface for email data access operations.
/// </summary>
public interface IEmailRepository
{
    // Email Log operations
    Task AddEmailLogAsync(EmailLog emailLog, CancellationToken cancellationToken = default);
    Task UpdateEmailLogAsync(EmailLog emailLog, CancellationToken cancellationToken = default);
    Task<EmailLog?> GetEmailLogAsync(int id, CancellationToken cancellationToken = default);
    Task<List<EmailLog>> GetEmailLogsAsync(string? userId = null, int? orderId = null, CancellationToken cancellationToken = default);
    Task<List<EmailLog>> GetFailedEmailLogsAsync(CancellationToken cancellationToken = default);

    // Email Template operations
    Task<EmailTemplate> AddTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
    Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetTemplateAsync(int id, CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetTemplateByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<EmailTemplate>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteTemplateAsync(int id, CancellationToken cancellationToken = default);
}
