namespace Mango.MessageBus.Events;

/// <summary>
/// Event published when an order is completed/delivered.
/// Consumed by Reward Service to calculate and award reward points.
/// </summary>
public class OrderCompletedEvent
{
    public int OrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public DateTime CompletionDate { get; set; } = DateTime.UtcNow;
}
