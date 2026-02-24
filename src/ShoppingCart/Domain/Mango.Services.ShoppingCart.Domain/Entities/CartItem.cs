namespace Mango.Services.ShoppingCart.Domain.Entities;

/// <summary>
/// Represents a single item (product) in a shopping cart.
/// </summary>
public class CartItem : BaseEntity
{
    /// <summary>
    /// The shopping cart this item belongs to.
    /// </summary>
    public int ShoppingCartId { get; set; }

    /// <summary>
    /// Product ID from the Product Service (foreign key reference).
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Product name (cached for display without calling Product Service).
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product price at the time of adding to cart.
    /// </summary>
    public decimal ProductPrice { get; set; }

    /// <summary>
    /// Product image URL for display.
    /// </summary>
    public string? ProductImageUrl { get; set; }

    /// <summary>
    /// Quantity of this product in the cart.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Calculated total for this line item (Price × Quantity).
    /// </summary>
    public decimal LineTotal => ProductPrice * Quantity;

    /// <summary>
    /// Check if the item quantity is valid for purchase.
    /// </summary>
    public bool IsValidQuantity() => Quantity > 0;

    /// <summary>
    /// Update the quantity of the item.
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

        Quantity = newQuantity;
    }
}
