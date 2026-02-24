namespace Mango.Services.Reward.Domain.Entities;

/// <summary>
/// Reward tier entity. Defines membership levels (Bronze, Silver, Gold, Platinum).
/// </summary>
public class RewardTier : AuditableEntity
{
    public string Name { get; set; } = string.Empty; // Bronze, Silver, Gold, Platinum
    public int MinimumPoints { get; set; }
    public int MaximumPoints { get; set; }
    public decimal BonusMultiplier { get; set; } = 1.0m; // 1.0x for Bronze, 1.5x for Silver, 2.0x for Gold, 3.0x for Platinum
    public string Benefits { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               MinimumPoints >= 0 &&
               MaximumPoints > MinimumPoints &&
               BonusMultiplier > 0;
    }

    public static List<RewardTier> CreateDefaultTiers()
    {
        return new List<RewardTier>
        {
            new RewardTier
            {
                Name = "Bronze",
                MinimumPoints = 0,
                MaximumPoints = 499,
                BonusMultiplier = 1.0m,
                Benefits = "Basic rewards",
                IsActive = true
            },
            new RewardTier
            {
                Name = "Silver",
                MinimumPoints = 500,
                MaximumPoints = 999,
                BonusMultiplier = 1.5m,
                Benefits = "1.5x points on purchases",
                IsActive = true
            },
            new RewardTier
            {
                Name = "Gold",
                MinimumPoints = 1000,
                MaximumPoints = 4999,
                BonusMultiplier = 2.0m,
                Benefits = "2x points on purchases, exclusive discounts",
                IsActive = true
            },
            new RewardTier
            {
                Name = "Platinum",
                MinimumPoints = 5000,
                MaximumPoints = int.MaxValue,
                BonusMultiplier = 3.0m,
                Benefits = "3x points on purchases, VIP support, exclusive deals",
                IsActive = true
            }
        };
    }
}
