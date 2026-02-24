namespace Mango.Services.Email.Application.DTOs;

/// <summary>
/// Data transfer object for email logs.
/// </summary>
public class EmailLogDto
{
    public int Id { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TemplateName { get; set; }
    public bool IsSent { get; set; }
    public int AttemptCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public int? OrderId { get; set; }
    public string? UserId { get; set; }
    public string? EmailType { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for sending custom emails.
/// </summary>
public class SendEmailRequest
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TemplateName { get; set; }
    public Dictionary<string, string>? TemplateVariables { get; set; }
    public string? EmailType { get; set; }
    public int? OrderId { get; set; }
    public string? UserId { get; set; }
}

/// <summary>
/// Response DTO for email sending result.
/// </summary>
public class SendEmailResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? EmailLogId { get; set; }
    public DateTime? SentAt { get; set; }
}
