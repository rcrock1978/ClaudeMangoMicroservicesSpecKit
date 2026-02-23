using Mango.MessageBus.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Mango.Services.Email.Infrastructure.Consumers;

/// <summary>
/// Handles OrderPlacedEvent to send order confirmation email.
/// Idempotent: safe to process the same event multiple times.
/// </summary>
public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedEventConsumer> _logger;
    // TODO: Inject email service to send emails

    public OrderPlacedEventConsumer(ILogger<OrderPlacedEventConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "OrderPlacedEvent received - OrderId: {OrderId}, UserId: {UserId}, Email: {Email}",
                @event.OrderHeaderId, @event.UserId, @event.Email);

            // Idempotency check: ensure this event hasn't been processed before
            // TODO: Check database for existing EmailLog entry with same OrderHeaderId
            // If exists, log as duplicate and return

            // Send order placed email
            // TODO: Build email template with order details
            // Email template should include:
            // - Order ID
            // - Order total
            // - Discount applied
            // - Items list
            // - Estimated delivery
            // - Customer details

            _logger.LogInformation("Order placed email sent to {Email}", @event.Email);

            // TODO: Create EmailLog record in database for audit trail
            // Include: Email address, event type, status, timestamp

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderPlacedEvent for order {OrderId}", @event.OrderHeaderId);
            // MassTransit will handle retry based on configuration (3 attempts with backoff)
            throw;
        }
    }
}
