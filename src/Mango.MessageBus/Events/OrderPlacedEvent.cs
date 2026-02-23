namespace Mango.MessageBus.Events;

public class OrderPlacedEvent
{
    public int OrderHeaderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal OrderTotalBeforeDiscount { get; set; }
    public string? CouponCode { get; set; }
    public DateTime OrderTime { get; set; } = DateTime.UtcNow;
    public List<OrderPlacedItem> OrderDetails { get; set; } = new();

    public class OrderPlacedItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Count { get; set; }
    }
}
