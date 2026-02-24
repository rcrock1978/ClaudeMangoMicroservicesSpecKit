namespace Mango.Services.Reward.Domain.Entities;

/// <summary>
/// Reward point transaction entity. Tracks all reward point changes for users.
/// </summary>
public class RewardPoint : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public int Points { get; set; }
    public string TransactionType { get; set; } = string.Empty; // "Earned" or "Redeemed"
    public string Description { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public decimal? OrderAmount { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               Points > 0 &&
               !string.IsNullOrWhiteSpace(TransactionType);
    }
}
