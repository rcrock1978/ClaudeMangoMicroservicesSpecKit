using Mango.Services.Email.Application.DTOs;
using Mango.Services.Email.Application.Interfaces;
using Mango.Services.Email.Application.MediatR.Commands;
using MediatR;

namespace Mango.Services.Email.Application.MediatR.Handlers;

/// <summary>
/// Handler for SendEmailCommand.
/// </summary>
public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, SendEmailResponse>
{
    private readonly IEmailService _emailService;

    public SendEmailCommandHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<SendEmailResponse> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        // If template is specified and variables provided, use template rendering
        if (!string.IsNullOrEmpty(request.TemplateName) && request.TemplateVariables != null)
        {
            return await _emailService.SendEmailWithTemplateAsync(
                request.TemplateName,
                request.RecipientEmail,
                request.TemplateVariables,
                request.RecipientName,
                request.OrderId,
                request.UserId,
                cancellationToken);
        }

        // Otherwise send plain email
        var sendRequest = new SendEmailRequest
        {
            RecipientEmail = request.RecipientEmail,
            RecipientName = request.RecipientName,
            Subject = request.Subject,
            Body = request.Body,
            TemplateName = request.TemplateName,
            EmailType = request.EmailType,
            OrderId = request.OrderId,
            UserId = request.UserId
        };

        return await _emailService.SendEmailAsync(sendRequest, cancellationToken);
    }
}
