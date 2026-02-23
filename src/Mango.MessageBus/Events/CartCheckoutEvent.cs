namespace Mango.MessageBus.Events;

public class CartCheckoutEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public decimal CartTotal { get; set; }
    public List<CartCheckoutItem> CartDetails { get; set; } = new();
    public string StripeSessionId { get; set; } = string.Empty;

    public class CartCheckoutItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Count { get; set; }
    }
}
