namespace Mango.Services.ShoppingCart.Domain.Entities;

/// <summary>
/// Base entity with audit tracking for all domain entities.
/// Provides common properties for all entities in the domain.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// When the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the entity was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// User identifier who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User identifier who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
