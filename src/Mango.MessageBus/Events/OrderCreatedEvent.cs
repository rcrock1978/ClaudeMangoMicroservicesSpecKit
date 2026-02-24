namespace Mango.MessageBus.Events;

/// <summary>
/// Event published when an order is created.
/// Consumed by Email Service to send order confirmation.
/// </summary>
public class OrderCreatedEvent
{
    public int OrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
}
