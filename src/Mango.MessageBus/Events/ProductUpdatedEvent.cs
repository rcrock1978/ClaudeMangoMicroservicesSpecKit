namespace Mango.MessageBus.Events;

public class ProductUpdatedEvent
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
