using Mango.Services.Email.Application.DTOs;
using Mango.Services.Email.Application.MediatR;

namespace Mango.Services.Email.Application.MediatR.Commands;

/// <summary>
/// Command to send an email.
/// </summary>
public class SendEmailCommand : BaseCommand<SendEmailResponse>
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
