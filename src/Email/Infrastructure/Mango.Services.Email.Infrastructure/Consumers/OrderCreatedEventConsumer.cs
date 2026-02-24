using Mango.MessageBus.Events;
using Mango.Services.Email.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Mango.Services.Email.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer for OrderCreatedEvent.
/// Sends order confirmation email when an order is created.
/// </summary>
public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedEventConsumer> _logger;

    public OrderCreatedEventConsumer(IEmailService emailService, ILogger<OrderCreatedEventConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        try
        {
            _logger.LogInformation("Processing OrderCreatedEvent for Order ID: {OrderId}", context.Message.OrderId);

            // Prepare variables for email template
            var variables = new Dictionary<string, string>
            {
                { "CustomerName", context.Message.CustomerName },
                { "OrderId", context.Message.OrderId.ToString() },
                { "OrderTotal", context.Message.OrderTotal.ToString("C") },
                { "OrderDate", context.Message.OrderDate.ToString("MMM dd, yyyy HH:mm") }
            };

            // Send order confirmation email using template
            var result = await _emailService.SendEmailWithTemplateAsync(
                templateName: "OrderConfirmation",
                recipientEmail: context.Message.CustomerEmail,
                variables: variables,
                recipientName: context.Message.CustomerName,
                orderId: context.Message.OrderId,
                userId: context.Message.UserId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Order confirmation email sent successfully for Order ID: {OrderId}", context.Message.OrderId);
            }
            else
            {
                _logger.LogWarning("Failed to send order confirmation email for Order ID: {OrderId}. Reason: {Message}",
                    context.Message.OrderId, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderCreatedEvent for Order ID: {OrderId}", context.Message.OrderId);
            throw;
        }
    }
}
