using Mango.Services.Email.Application.DTOs;

namespace Mango.Services.Email.Application.Interfaces;

/// <summary>
/// Service interface for sending emails and managing email templates.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email asynchronously.
    /// </summary>
    Task<SendEmailResponse> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send email using a template.
    /// </summary>
    Task<SendEmailResponse> SendEmailWithTemplateAsync(string templateName, string recipientEmail, Dictionary<string, string> variables, string? recipientName = null, int? orderId = null, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template by name.
    /// </summary>
    Task<EmailTemplateDto?> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active templates.
    /// </summary>
    Task<List<EmailTemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new email template.
    /// </summary>
    Task<EmailTemplateDto> CreateTemplateAsync(CreateEmailTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing email template.
    /// </summary>
    Task<EmailTemplateDto> UpdateTemplateAsync(UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get email logs for a specific user.
    /// </summary>
    Task<List<EmailLogDto>> GetEmailLogsAsync(string? userId = null, int? orderId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry sending failed emails.
    /// </summary>
    Task RetryFailedEmailsAsync(int maxRetries = 3, CancellationToken cancellationToken = default);
}
