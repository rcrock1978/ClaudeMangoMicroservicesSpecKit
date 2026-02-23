namespace Mango.Services.Product.Domain.Entities;

/// <summary>
/// Product catalog entity.
/// Represents a sellable product with pricing, availability, and category association.
/// Supports soft delete for historical tracking.
/// </summary>
public class Product : AuditableEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Query helper to exclude deleted products.
    /// </summary>
    public static IQueryable<Product> GetActive(IQueryable<Product> query)
        => query.Where(p => !p.IsDeleted && p.IsAvailable);

    /// <summary>
    /// Check if product is available for purchase.
    /// </summary>
    public bool CanBePurchased() => IsAvailable && !IsDeleted && Price > 0;

    /// <summary>
    /// Calculate total cost for given quantity.
    /// </summary>
    public decimal CalculateTotal(int quantity) => Price * quantity;
}
