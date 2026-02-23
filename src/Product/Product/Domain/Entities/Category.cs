namespace Mango.Services.Product.Domain.Entities;

/// <summary>
/// Product category entity.
/// Supports soft delete for historical tracking.
/// </summary>
public class Category : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Query helper to exclude deleted categories.
    /// </summary>
    public static IQueryable<Category> GetActive(IQueryable<Category> query)
        => query.Where(c => !c.IsDeleted);
}
