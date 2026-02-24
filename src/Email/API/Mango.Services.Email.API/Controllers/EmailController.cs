using Mango.Services.Email.Application.DTOs;
using Mango.Services.Email.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.Email.API.Controllers;

/// <summary>
/// API controller for email operations.
/// Provides endpoints for sending emails, managing templates, and viewing email logs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Get all email templates.
    /// GET /api/email/templates
    /// </summary>
    [HttpGet("templates")]
    [ProduceResponseType(typeof(ResponseDto<List<EmailTemplateDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTemplates(CancellationToken cancellationToken)
    {
        try
        {
            var templates = await _emailService.GetAllTemplatesAsync(cancellationToken);
            return Ok(ResponseDto<List<EmailTemplateDto>>.Success(templates, "Templates retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email templates");
            return StatusCode(500, ResponseDto<object>.Error("An error occurred while retrieving templates", 500));
        }
    }

    /// <summary>
    /// Get a specific email template by name.
    /// GET /api/email/templates/{templateName}
    /// </summary>
    [HttpGet("templates/{templateName}")]
    [ProduceResponseType(typeof(ResponseDto<EmailTemplateDto>), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(string templateName, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _emailService.GetTemplateAsync(templateName, cancellationToken);
            if (template == null)
            {
                return NotFound(ResponseDto.Error($"Template '{templateName}' not found", 404));
            }

            return Ok(ResponseDto<EmailTemplateDto>.Success(template, "Template retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template: {TemplateName}", templateName);
            return StatusCode(500, ResponseDto.Error("An error occurred while retrieving template", 500));
        }
    }

    /// <summary>
    /// Create a new email template.
    /// POST /api/email/templates
    /// </summary>
    [HttpPost("templates")]
    [ProduceResponseType(typeof(ResponseDto<EmailTemplateDto>), StatusCodes.Status201Created)]
    [ProduceResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto.Error("Invalid request data", 400));
            }

            var template = await _emailService.CreateTemplateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetTemplate), new { templateName = template.Name },
                ResponseDto<EmailTemplateDto>.Success(template, "Template created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email template");
            return StatusCode(500, ResponseDto.Error("An error occurred while creating template", 500));
        }
    }

    /// <summary>
    /// Update an existing email template.
    /// PUT /api/email/templates/{id}
    /// </summary>
    [HttpPut("templates/{id}")]
    [ProduceResponseType(typeof(ResponseDto<EmailTemplateDto>), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpdateEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (id != request.Id)
            {
                return BadRequest(ResponseDto.Error("ID mismatch", 400));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto.Error("Invalid request data", 400));
            }

            var template = await _emailService.UpdateTemplateAsync(request, cancellationToken);
            return Ok(ResponseDto<EmailTemplateDto>.Success(template, "Template updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email template: {TemplateId}", id);
            return StatusCode(500, ResponseDto.Error("An error occurred while updating template", 500));
        }
    }

    /// <summary>
    /// Send a custom email.
    /// POST /api/email/send
    /// </summary>
    [HttpPost("send")]
    [ProduceResponseType(typeof(ResponseDto<SendEmailResponse>), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto.Error("Invalid request data", 400));
            }

            var result = await _emailService.SendEmailAsync(request, cancellationToken);
            if (!result.IsSuccess)
            {
                return BadRequest(ResponseDto<SendEmailResponse>.Error(result.Message, 400));
            }

            return Ok(ResponseDto<SendEmailResponse>.Success(result, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to: {Email}", request.RecipientEmail);
            return StatusCode(500, ResponseDto.Error("An error occurred while sending email", 500));
        }
    }

    /// <summary>
    /// Get email logs for a specific user or order.
    /// GET /api/email/logs?userId={userId}&orderId={orderId}
    /// </summary>
    [HttpGet("logs")]
    [ProduceResponseType(typeof(ResponseDto<List<EmailLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmailLogs([FromQuery] string? userId = null, [FromQuery] int? orderId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _emailService.GetEmailLogsAsync(userId, orderId, cancellationToken);
            return Ok(ResponseDto<List<EmailLogDto>>.Success(logs, "Email logs retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email logs for userId: {UserId}, orderId: {OrderId}", userId, orderId);
            return StatusCode(500, ResponseDto.Error("An error occurred while retrieving email logs", 500));
        }
    }

    /// <summary>
    /// Retry sending failed emails.
    /// POST /api/email/retry-failed
    /// </summary>
    [HttpPost("retry-failed")]
    [ProduceResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RetryFailedEmails(CancellationToken cancellationToken)
    {
        try
        {
            await _emailService.RetryFailedEmailsAsync(maxRetries: 3, cancellationToken);
            return Ok(ResponseDto.Success(null, "Failed emails retry process completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed emails");
            return StatusCode(500, ResponseDto.Error("An error occurred during retry process", 500));
        }
    }
}
