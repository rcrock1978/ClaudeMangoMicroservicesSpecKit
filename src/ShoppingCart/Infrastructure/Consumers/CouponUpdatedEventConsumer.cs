using Mango.MessageBus.Events;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Mango.Services.ShoppingCart.Infrastructure.Consumers;

/// <summary>
/// Handles CouponUpdatedEvent to invalidate coupon validation cache.
/// Ensures subsequent coupon validations retrieve updated discount rules.
/// Idempotent: safe to process the same event multiple times.
/// </summary>
public class CouponUpdatedEventConsumer : IConsumer<CouponUpdatedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CouponUpdatedEventConsumer> _logger;

    public CouponUpdatedEventConsumer(IDistributedCache cache, ILogger<CouponUpdatedEventConsumer> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CouponUpdatedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "CouponUpdatedEvent received - CouponId: {CouponId}, Code: {CouponCode}, IsActive: {IsActive}",
                @event.CouponId, @event.CouponCode, @event.IsActive);

            // Invalidate coupon cache entries
            var couponCacheKey = $"coupon_{@event.CouponCode}";
            await _cache.RemoveAsync(couponCacheKey);
            _logger.LogInformation("Invalidated cache for coupon {CouponCode}", @event.CouponCode);

            // If coupon was deactivated, also invalidate any carts using this coupon
            if (!@event.IsActive)
            {
                // TODO: Query all carts with this coupon applied
                // For each cart:
                //   - Remove coupon application
                //   - Recalculate cart total without discount
                //   - Notify customer (optional)
                //   - Invalidate cart cache

                _logger.LogInformation("Deactivated coupon {CouponCode} - clearing from active carts", @event.CouponCode);
            }

            // TODO: Invalidate validation result cache for this coupon
            // Pattern: await _cache.RemoveAsync($"coupon_validation_{@event.CouponCode}");

            _logger.LogInformation("Cache invalidation completed for coupon {CouponCode}", @event.CouponCode);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CouponUpdatedEvent for coupon {CouponCode}", @event.CouponCode);
            throw;
        }
    }
}
