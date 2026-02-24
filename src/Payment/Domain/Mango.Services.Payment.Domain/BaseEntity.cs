namespace Mango.Services.Payment.Domain;

/// <summary>
/// Base entity class with common audit fields for all domain entities.
/// Provides created/updated timestamps and soft delete support.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// When the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the entity was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = "system";

    /// <summary>
    /// User who last updated the entity.
    /// </summary>
    public string UpdatedBy { get; set; } = "system";

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the entity was deleted (UTC), if applicable.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
