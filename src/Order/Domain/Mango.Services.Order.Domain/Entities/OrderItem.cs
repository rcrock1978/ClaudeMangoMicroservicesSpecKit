namespace Mango.Services.Order.Domain.Entities;

/// <summary>
/// Represents a line item in an order.
/// Contains product details, quantity, and pricing information.
/// </summary>
public class OrderItem : BaseEntity
{
    /// <summary>
    /// Reference to the parent Order (foreign key).
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Product ID for reference (not a direct foreign key to Product service).
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Product name captured at time of order (denormalized).
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Unit price at time of order (may differ from current product price).
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Quantity ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Total price for this line item (UnitPrice * Quantity).
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Discount applied to this specific item (if any).
    /// </summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// Calculate the total for this item (UnitPrice * Quantity).
    /// </summary>
    public decimal CalculateTotal()
    {
        return UnitPrice * Quantity;
    }

    /// <summary>
    /// Calculate the final price after discount.
    /// </summary>
    public decimal CalculateFinalPrice()
    {
        var total = CalculateTotal();
        return Math.Max(total - DiscountAmount, 0);
    }

    /// <summary>
    /// Validate order item data.
    /// </summary>
    public bool IsValid()
    {
        return ProductId > 0 &&
               !string.IsNullOrWhiteSpace(ProductName) &&
               UnitPrice > 0 &&
               Quantity > 0 &&
               DiscountAmount >= 0;
    }
}
