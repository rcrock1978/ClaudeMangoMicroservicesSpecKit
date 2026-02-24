using Serilog;

namespace Mango.Services.Admin.Infrastructure.HttpClients;

/// <summary>
/// HTTP client for communicating with the Reward microservice.
/// </summary>
public interface IRewardServiceClient
{
    /// <summary>Gets customer reward statistics.</summary>
    Task<CustomerRewardStatsDto?> GetCustomerRewardStatsAsync(string userId);

    /// <summary>Gets all customer reward tiers and their counts.</summary>
    Task<List<RewardTierStatsDto>?> GetRewardTierStatsAsync();

    /// <summary>Gets total points issued and redeemed.</summary>
    Task<RewardSummaryDto?> GetRewardSummaryAsync();

    /// <summary>Gets top customers by reward points.</summary>
    Task<List<TopRewardsCustomerDto>?> GetTopCustomersByRewardsAsync(int count = 10);

    /// <summary>Gets customers with reward tier distribution.</summary>
    Task<CustomerTierDistributionDto?> GetCustomerTierDistributionAsync();

    /// <summary>Gets engagement metrics for rewards program.</summary>
    Task<RewardEngagementDto?> GetRewardEngagementAsync();
}

public class RewardServiceClient : BaseServiceClient, IRewardServiceClient
{
    public RewardServiceClient(HttpClient httpClient, ILogger logger)
        : base(httpClient, logger)
    {
    }

    public async Task<CustomerRewardStatsDto?> GetCustomerRewardStatsAsync(string userId)
    {
        _logger.Information("Fetching reward stats for customer {UserId}", userId);
        return await GetAsync<CustomerRewardStatsDto>($"/api/rewards/customers/{userId}/stats");
    }

    public async Task<List<RewardTierStatsDto>?> GetRewardTierStatsAsync()
    {
        _logger.Information("Fetching reward tier statistics");
        return await GetAsync<List<RewardTierStatsDto>>("/api/rewards/tiers/stats");
    }

    public async Task<RewardSummaryDto?> GetRewardSummaryAsync()
    {
        _logger.Information("Fetching reward program summary");
        return await GetAsync<RewardSummaryDto>("/api/rewards/summary");
    }

    public async Task<List<TopRewardsCustomerDto>?> GetTopCustomersByRewardsAsync(int count = 10)
    {
        _logger.Information("Fetching top {Count} customers by rewards", count);
        return await GetAsync<List<TopRewardsCustomerDto>>($"/api/rewards/top-customers?count={count}");
    }

    public async Task<CustomerTierDistributionDto?> GetCustomerTierDistributionAsync()
    {
        _logger.Information("Fetching customer tier distribution");
        return await GetAsync<CustomerTierDistributionDto>("/api/rewards/tier-distribution");
    }

    public async Task<RewardEngagementDto?> GetRewardEngagementAsync()
    {
        _logger.Information("Fetching reward engagement metrics");
        return await GetAsync<RewardEngagementDto>("/api/rewards/engagement");
    }
}

/// <summary>DTO for individual customer reward statistics.</summary>
public class CustomerRewardStatsDto
{
    public string UserId { get; set; } = string.Empty;
    public int TotalPointsEarned { get; set; }
    public int PointsRedeemed { get; set; }
    public int PointsRemaining { get; set; }
    public string CurrentTier { get; set; } = string.Empty;
    public decimal TierBonusMultiplier { get; set; }
    public int PurchaseCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
}

/// <summary>DTO for reward tier statistics.</summary>
public class RewardTierStatsDto
{
    public string TierName { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public int TotalPointsEarned { get; set; }
    public decimal BonusMultiplier { get; set; }
    public int MinimumPointsRequired { get; set; }
}

/// <summary>DTO for rewards program summary.</summary>
public class RewardSummaryDto
{
    public int TotalCustomersEnrolled { get; set; }
    public int ActiveParticipants { get; set; }
    public int TotalPointsIssued { get; set; }
    public int TotalPointsRedeemed { get; set; }
    public int OutstandingPoints { get; set; }
    public decimal AveragePointsPerCustomer { get; set; }
    public decimal RedemptionRate { get; set; }
}

/// <summary>DTO for top customers by rewards.</summary>
public class TopRewardsCustomerDto
{
    public string UserId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public string CurrentTier { get; set; } = string.Empty;
    public int PurchaseCount { get; set; }
    public decimal TotalSpent { get; set; }
}

/// <summary>DTO for customer tier distribution.</summary>
public class CustomerTierDistributionDto
{
    public int BronzeTierCount { get; set; }
    public int SilverTierCount { get; set; }
    public int GoldTierCount { get; set; }
    public int PlatinumTierCount { get; set; }
    public int TotalEnrolledCustomers { get; set; }
    public List<TierPercentageDto> TierPercentages { get; set; } = new();
}

/// <summary>DTO for tier percentage breakdown.</summary>
public class TierPercentageDto
{
    public string TierName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>DTO for reward engagement metrics.</summary>
public class RewardEngagementDto
{
    public decimal EngagementRate { get; set; }
    public int ParticipationGrowthThisMonth { get; set; }
    public decimal AveragePointsEarnedPerTransaction { get; set; }
    public int TotalPointsRedeemedThisMonth { get; set; }
    public decimal RedemptionValueThisMonth { get; set; }
}
