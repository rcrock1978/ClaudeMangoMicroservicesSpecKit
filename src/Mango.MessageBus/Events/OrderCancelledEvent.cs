namespace Mango.MessageBus.Events;

public class OrderCancelledEvent
{
    public int OrderHeaderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CancelledBy { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; } = DateTime.UtcNow;
}
