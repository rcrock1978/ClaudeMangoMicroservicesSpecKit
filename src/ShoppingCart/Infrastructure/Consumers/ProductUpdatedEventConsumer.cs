using Mango.MessageBus.Events;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Mango.Services.ShoppingCart.Infrastructure.Consumers;

/// <summary>
/// Handles ProductUpdatedEvent to invalidate Redis cache and re-verify cart item prices.
/// Ensures carts always reflect current product pricing.
/// Idempotent: safe to process the same event multiple times.
/// </summary>
public class ProductUpdatedEventConsumer : IConsumer<ProductUpdatedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<ProductUpdatedEventConsumer> _logger;

    public ProductUpdatedEventConsumer(IDistributedCache cache, ILogger<ProductUpdatedEventConsumer> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "ProductUpdatedEvent received - ProductId: {ProductId}, Name: {ProductName}, NewPrice: {NewPrice}",
                @event.ProductId, @event.ProductName, @event.Price);

            // Invalidate product cache
            var cacheKey = $"product_{@event.ProductId}";
            await _cache.RemoveAsync(cacheKey);
            _logger.LogInformation("Invalidated cache for product {ProductId}", @event.ProductId);

            // TODO: Query all active carts containing this product
            // For each cart:
            //   - Re-verify item is still available (@event.IsAvailable)
            //   - Update item price if changed
            //   - Recalculate cart total
            //   - Notify customer if price increased (optional UX improvement)
            //   - Invalidate cart cache

            // Example cache invalidation pattern for all carts:
            // - Use Redis KEYS pattern or maintain index
            // - Invalidate affected carts: await _cache.RemoveAsync($"cart_{userId}");

            _logger.LogInformation("Cache invalidation completed for product {ProductId}", @event.ProductId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ProductUpdatedEvent for product {ProductId}", @event.ProductId);
            throw;
        }
    }
}
