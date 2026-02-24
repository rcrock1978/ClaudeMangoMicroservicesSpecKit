namespace Mango.Services.Order.Domain.Entities;

/// <summary>
/// Enumeration of order states in the order lifecycle.
/// Defines all valid states for an order from creation to completion or cancellation.
/// </summary>
public enum OrderStatus
{
    /// <summary>Order created but not yet confirmed for payment processing.</summary>
    Pending = 1,

    /// <summary>Order confirmed and payment is being processed.</summary>
    Processing = 2,

    /// <summary>Order payment confirmed, shipped to customer.</summary>
    Shipped = 3,

    /// <summary>Order delivered to customer.</summary>
    Delivered = 4,

    /// <summary>Order cancelled by customer or system.</summary>
    Cancelled = 5
}
