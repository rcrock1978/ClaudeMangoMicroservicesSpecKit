using Mango.MessageBus.Events;
using Mango.Services.Reward.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Mango.Services.Reward.Infrastructure.Consumers;

public class OrderCompletedEventConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly IRewardService _rewardService;
    private readonly ILogger<OrderCompletedEventConsumer> _logger;

    public OrderCompletedEventConsumer(IRewardService rewardService, ILogger<OrderCompletedEventConsumer> logger)
    {
        _rewardService = rewardService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        try
        {
            _logger.LogInformation("Processing OrderCompletedEvent for Order ID: {OrderId}, User: {UserId}",
                context.Message.OrderId, context.Message.UserId);

            // Calculate and award reward points
            var points = await _rewardService.CalculatePointsAsync(
                context.Message.OrderTotal,
                context.Message.UserId,
                context.Message.OrderId);

            // Add the points to user account
            var success = await _rewardService.AddPointsAsync(
                context.Message.UserId,
                points,
                $"Reward points earned from order {context.Message.OrderId}",
                context.Message.OrderId,
                context.Message.OrderTotal);

            if (success)
            {
                _logger.LogInformation("Awarded {Points} reward points to user {UserId} for order {OrderId}",
                    points, context.Message.UserId, context.Message.OrderId);
            }
            else
            {
                _logger.LogWarning("Failed to award reward points for order {OrderId}", context.Message.OrderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderCompletedEvent for Order ID: {OrderId}",
                context.Message.OrderId);
            throw;
        }
    }
}
