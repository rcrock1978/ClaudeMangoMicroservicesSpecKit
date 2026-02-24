namespace Mango.Services.Reward.Application.DTOs;

public class RewardPointDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Points { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class UserRewardsDto
{
    public string UserId { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public string CurrentTier { get; set; } = string.Empty;
    public decimal BonusMultiplier { get; set; }
    public List<RewardPointDto> RecentTransactions { get; set; } = new();
}

public class RewardTierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinimumPoints { get; set; }
    public int MaximumPoints { get; set; }
    public decimal BonusMultiplier { get; set; }
    public string Benefits { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CalculateRewardRequest
{
    public string UserId { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public int OrderId { get; set; }
}

public class RedeemRewardRequest
{
    public string UserId { get; set; } = string.Empty;
    public int PointsToRedeem { get; set; }
}
