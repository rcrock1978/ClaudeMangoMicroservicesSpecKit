namespace Mango.Services.ShoppingCart.Application.DTOs;

/// <summary>
/// DTO for cart item in API responses.
/// </summary>
public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => ProductPrice * Quantity;
}

/// <summary>
/// DTO for shopping cart in API responses.
/// </summary>
public class ShoppingCartDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<CartItemDto> Items { get; set; } = new();
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}

/// <summary>
/// Request DTO for adding item to cart.
/// </summary>
public class AddToCartRequest
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Request DTO for updating cart item quantity.
/// </summary>
public class UpdateCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Request DTO for applying coupon to cart.
/// </summary>
public class ApplyCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
}
