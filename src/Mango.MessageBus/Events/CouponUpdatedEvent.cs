namespace Mango.MessageBus.Events;

public class CouponUpdatedEvent
{
    public int CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public int MinAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
