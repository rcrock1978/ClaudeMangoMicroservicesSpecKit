using Mango.Services.Email.Application.DTOs;
using Mango.Services.Email.Application.Interfaces;
using Mango.Services.Email.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Mango.Services.Email.Application.Services;

/// <summary>
/// Service for sending emails via SMTP and managing email templates.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IEmailRepository _repository;
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _settings;

    public EmailService(IEmailRepository repository, ILogger<EmailService> logger, EmailSettings settings)
    {
        _repository = repository;
        _logger = logger;
        _settings = settings;
    }

    public async Task<SendEmailResponse> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.RecipientEmail))
            {
                return new SendEmailResponse
                {
                    IsSuccess = false,
                    Message = "Recipient email is required"
                };
            }

            // Create email log entry
            var emailLog = new EmailLog
            {
                RecipientEmail = request.RecipientEmail,
                RecipientName = request.RecipientName,
                Subject = request.Subject,
                Body = request.Body,
                TemplateName = request.TemplateName,
                EmailType = request.EmailType,
                OrderId = request.OrderId,
                UserId = request.UserId,
                AttemptCount = 0
            };

            // Send the email
            var sendResult = await SendSmtpEmailAsync(request.RecipientEmail, request.Subject, request.Body, cancellationToken);

            if (sendResult)
            {
                emailLog.MarkAsSent();
                _logger.LogInformation("Email sent successfully to {Email}", request.RecipientEmail);
            }
            else
            {
                emailLog.RecordFailedAttempt("SMTP send failed");
                _logger.LogWarning("Failed to send email to {Email}", request.RecipientEmail);
            }

            // Log the email attempt
            await _repository.AddEmailLogAsync(emailLog, cancellationToken);

            return new SendEmailResponse
            {
                IsSuccess = sendResult,
                Message = sendResult ? "Email sent successfully" : "Failed to send email",
                EmailLogId = emailLog.Id,
                SentAt = emailLog.SentAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", request.RecipientEmail);
            return new SendEmailResponse
            {
                IsSuccess = false,
                Message = "An error occurred while sending email"
            };
        }
    }

    public async Task<SendEmailResponse> SendEmailWithTemplateAsync(string templateName, string recipientEmail, Dictionary<string, string> variables, string? recipientName = null, int? orderId = null, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get template
            var template = await _repository.GetTemplateByNameAsync(templateName, cancellationToken);
            if (template == null)
            {
                _logger.LogWarning("Email template not found: {TemplateName}", templateName);
                return new SendEmailResponse
                {
                    IsSuccess = false,
                    Message = $"Email template '{templateName}' not found"
                };
            }

            // Render template with variables
            var renderedBody = template.RenderTemplate(variables);
            var renderedSubject = template.Subject;
            foreach (var kvp in variables)
            {
                renderedSubject = renderedSubject.Replace($"{{{kvp.Key}}}", kvp.Value ?? string.Empty);
            }

            // Send email
            var request = new SendEmailRequest
            {
                RecipientEmail = recipientEmail,
                RecipientName = recipientName,
                Subject = renderedSubject,
                Body = renderedBody,
                TemplateName = templateName,
                EmailType = templateName,
                OrderId = orderId,
                UserId = userId
            };

            return await SendEmailAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template email {TemplateName} to {Email}", templateName, recipientEmail);
            return new SendEmailResponse
            {
                IsSuccess = false,
                Message = "An error occurred while sending email"
            };
        }
    }

    public async Task<EmailTemplateDto?> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetTemplateByNameAsync(templateName, cancellationToken);
        return template == null ? null : MapToDto(template);
    }

    public async Task<List<EmailTemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _repository.GetAllTemplatesAsync(cancellationToken);
        return templates.Select(MapToDto).ToList();
    }

    public async Task<EmailTemplateDto> CreateTemplateAsync(CreateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = new EmailTemplate
        {
            Name = request.Name,
            Subject = request.Subject,
            Body = request.Body,
            Variables = request.Variables,
            IsActive = request.IsActive,
            Description = request.Description
        };

        var created = await _repository.AddTemplateAsync(template, cancellationToken);
        return MapToDto(created);
    }

    public async Task<EmailTemplateDto> UpdateTemplateAsync(UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetTemplateAsync(request.Id, cancellationToken);
        if (template == null)
        {
            throw new Exception($"Template with ID {request.Id} not found");
        }

        template.Name = request.Name;
        template.Subject = request.Subject;
        template.Body = request.Body;
        template.Variables = request.Variables;
        template.IsActive = request.IsActive;
        template.Description = request.Description;
        template.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateTemplateAsync(template, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<List<EmailLogDto>> GetEmailLogsAsync(string? userId = null, int? orderId = null, CancellationToken cancellationToken = default)
    {
        var logs = await _repository.GetEmailLogsAsync(userId, orderId, cancellationToken);
        return logs.Select(MapToDto).ToList();
    }

    public async Task RetryFailedEmailsAsync(int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        var failedLogs = await _repository.GetFailedEmailLogsAsync(cancellationToken);

        foreach (var log in failedLogs)
        {
            if (log.ShouldRetry(maxRetries))
            {
                _logger.LogInformation("Retrying email to {Email} (Attempt {Attempt})", log.RecipientEmail, log.AttemptCount + 1);

                var sendResult = await SendSmtpEmailAsync(log.RecipientEmail, log.Subject, log.Body, cancellationToken);

                if (sendResult)
                {
                    log.MarkAsSent();
                    _logger.LogInformation("Retry successful for email to {Email}", log.RecipientEmail);
                }
                else
                {
                    log.RecordFailedAttempt("SMTP retry failed");
                    _logger.LogWarning("Retry failed for email to {Email}", log.RecipientEmail);
                }

                await _repository.UpdateEmailLogAsync(log, cancellationToken);
            }
        }
    }

    private async Task<bool> SendSmtpEmailAsync(string recipientEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            using (var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort))
            {
                client.EnableSsl = _settings.EnableSsl;

                if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
                {
                    client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
                }

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_settings.FromEmail, _settings.FromName);
                    mailMessage.To.Add(recipientEmail);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;

                    await client.SendMailAsync(mailMessage, cancellationToken);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP error sending to {Email}", recipientEmail);
            return false;
        }
    }

    private EmailTemplateDto MapToDto(EmailTemplate template) => new()
    {
        Id = template.Id,
        Name = template.Name,
        Subject = template.Subject,
        Body = template.Body,
        Variables = template.Variables,
        IsActive = template.IsActive,
        Description = template.Description,
        CreatedAt = template.CreatedAt,
        UpdatedAt = template.UpdatedAt
    };

    private EmailLogDto MapToDto(EmailLog log) => new()
    {
        Id = log.Id,
        RecipientEmail = log.RecipientEmail,
        RecipientName = log.RecipientName,
        Subject = log.Subject,
        Body = log.Body,
        TemplateName = log.TemplateName,
        IsSent = log.IsSent,
        AttemptCount = log.AttemptCount,
        ErrorMessage = log.ErrorMessage,
        SentAt = log.SentAt,
        LastAttemptAt = log.LastAttemptAt,
        OrderId = log.OrderId,
        UserId = log.UserId,
        EmailType = log.EmailType,
        CreatedAt = log.CreatedAt
    };
}

/// <summary>
/// Email settings for SMTP configuration.
/// </summary>
public class EmailSettings
{
    public string SmtpServer { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 587;
    public string FromEmail { get; set; } = "noreply@mangoecommerce.com";
    public string FromName { get; set; } = "Mango E-Commerce";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = true;
}
