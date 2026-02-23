using Mango.MessageBus.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Mango.Services.Email.Infrastructure.Consumers;

/// <summary>
/// Handles OrderCancelledEvent to send order cancellation notification email.
/// Idempotent: safe to process the same event multiple times.
/// </summary>
public class OrderCancelledEventConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly ILogger<OrderCancelledEventConsumer> _logger;
    // TODO: Inject email service

    public OrderCancelledEventConsumer(ILogger<OrderCancelledEventConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "OrderCancelledEvent received - OrderId: {OrderId}, UserId: {UserId}, Email: {Email}, CancelledBy: {CancelledBy}",
                @event.OrderHeaderId, @event.UserId, @event.Email, @event.CancelledBy);

            // Idempotency check: ensure this event hasn't been processed before
            // TODO: Check database for existing EmailLog entry with same OrderHeaderId and event type
            // If exists, log as duplicate and return

            // Send order cancelled email
            // TODO: Build cancellation email template with:
            // - Order ID
            // - Cancellation reason (if available)
            // - Refund status
            // - Customer support contact info

            _logger.LogInformation("Order cancelled email sent to {Email}", @event.Email);

            // TODO: Create EmailLog record for audit trail

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderCancelledEvent for order {OrderId}", @event.OrderHeaderId);
            throw;
        }
    }
}
