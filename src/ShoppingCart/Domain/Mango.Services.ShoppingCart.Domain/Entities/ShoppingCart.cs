namespace Mango.Services.ShoppingCart.Domain.Entities;

/// <summary>
/// Represents a user's shopping cart with items ready for checkout.
/// </summary>
public class ShoppingCart : BaseEntity
{
    /// <summary>
    /// User ID who owns this cart.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Collection of items in the cart.
    /// </summary>
    public List<CartItem> Items { get; set; } = new();

    /// <summary>
    /// Optional coupon code applied to the cart.
    /// </summary>
    public string? CouponCode { get; set; }

    /// <summary>
    /// Discount amount applied by coupon (if any).
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Calculate the subtotal of all items in the cart.
    /// </summary>
    public decimal GetSubtotal() => Items.Sum(item => item.LineTotal);

    /// <summary>
    /// Calculate the total after discount.
    /// </summary>
    public decimal GetTotal()
    {
        var subtotal = GetSubtotal();
        return Math.Max(subtotal - DiscountAmount, 0);
    }

    /// <summary>
    /// Get the total number of items in the cart.
    /// </summary>
    public int GetItemCount() => Items.Sum(item => item.Quantity);

    /// <summary>
    /// Check if the cart is empty.
    /// </summary>
    public bool IsEmpty => !Items.Any();

    /// <summary>
    /// Check if the cart has valid items for checkout.
    /// </summary>
    public bool IsValidForCheckout() => !IsEmpty && Items.All(item => item.IsValidQuantity());

    /// <summary>
    /// Add an item to the cart. If product already exists, increase quantity.
    /// </summary>
    public void AddItem(int productId, string productName, decimal productPrice, string? productImageUrl, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var newItem = new CartItem
            {
                ProductId = productId,
                ProductName = productName,
                ProductPrice = productPrice,
                ProductImageUrl = productImageUrl,
                Quantity = quantity
            };
            Items.Add(newItem);
        }
    }

    /// <summary>
    /// Remove an item from the cart by product ID.
    /// </summary>
    public void RemoveItem(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    /// <summary>
    /// Update the quantity of an item in the cart.
    /// </summary>
    public void UpdateItemQuantity(int productId, int newQuantity)
    {
        if (newQuantity <= 0)
        {
            RemoveItem(productId);
            return;
        }

        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.UpdateQuantity(newQuantity);
        }
    }

    /// <summary>
    /// Clear all items from the cart.
    /// </summary>
    public void Clear()
    {
        Items.Clear();
        CouponCode = null;
        DiscountAmount = 0;
    }

    /// <summary>
    /// Apply a coupon code and discount to the cart.
    /// </summary>
    public void ApplyCoupon(string couponCode, decimal discountAmount)
    {
        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        CouponCode = couponCode;
        DiscountAmount = Math.Min(discountAmount, GetSubtotal()); // Discount can't exceed subtotal
    }

    /// <summary>
    /// Remove the applied coupon from the cart.
    /// </summary>
    public void RemoveCoupon()
    {
        CouponCode = null;
        DiscountAmount = 0;
    }
}
