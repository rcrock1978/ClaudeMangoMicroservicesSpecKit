using Mango.Services.Reward.Application.DTOs;
using Mango.Services.Reward.Application.Interfaces;
using Mango.Services.Reward.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Mango.Services.Reward.Application.Services;

public class RewardService : IRewardService
{
    private readonly IRewardRepository _repository;
    private readonly ILogger<RewardService> _logger;

    public RewardService(IRewardRepository repository, ILogger<RewardService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserRewardsDto> GetUserRewardsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var totalPoints = await _repository.GetUserTotalPointsAsync(userId, cancellationToken);
        var tier = await GetUserTierAsync(userId, cancellationToken);
        var recentTransactions = await _repository.GetUserRewardPointsAsync(userId, 10, cancellationToken);

        return new UserRewardsDto
        {
            UserId = userId,
            TotalPoints = totalPoints,
            CurrentTier = tier.Name,
            BonusMultiplier = tier.BonusMultiplier,
            RecentTransactions = recentTransactions.Select(MapToDto).ToList()
        };
    }

    public async Task<int> CalculatePointsAsync(decimal orderAmount, string userId, int orderId, CancellationToken cancellationToken = default)
    {
        // 1 point per $1 spent
        var basePoints = (int)Math.Floor(orderAmount);

        // Get user's tier for bonus multiplier
        var tier = await GetUserTierAsync(userId, cancellationToken);
        var earnedPoints = (int)Math.Floor(basePoints * tier.BonusMultiplier);

        _logger.LogInformation("Calculated {Points} reward points for user {UserId} from order {OrderId}",
            earnedPoints, userId, orderId);

        return earnedPoints;
    }

    public async Task<bool> AddPointsAsync(string userId, int points, string description, int? orderId = null, decimal? orderAmount = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var rewardPoint = new RewardPoint
            {
                UserId = userId,
                Points = points,
                TransactionType = "Earned",
                Description = description,
                OrderId = orderId,
                OrderAmount = orderAmount,
                TransactionDate = DateTime.UtcNow
            };

            if (!rewardPoint.IsValid())
            {
                _logger.LogWarning("Invalid reward point data for user {UserId}", userId);
                return false;
            }

            await _repository.AddRewardPointAsync(rewardPoint, cancellationToken);
            _logger.LogInformation("Added {Points} points to user {UserId}", points, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding points for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> RedeemPointsAsync(string userId, int points, CancellationToken cancellationToken = default)
    {
        try
        {
            var totalPoints = await _repository.GetUserTotalPointsAsync(userId, cancellationToken);

            if (totalPoints < points)
            {
                _logger.LogWarning("Insufficient points for user {UserId}. Available: {Available}, Requested: {Requested}",
                    userId, totalPoints, points);
                return false;
            }

            var rewardPoint = new RewardPoint
            {
                UserId = userId,
                Points = points,
                TransactionType = "Redeemed",
                Description = "Points redeemed for discount",
                TransactionDate = DateTime.UtcNow
            };

            await _repository.AddRewardPointAsync(rewardPoint, cancellationToken);
            _logger.LogInformation("Redeemed {Points} points for user {UserId}", points, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming points for user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<RewardTierDto>> GetAllTiersAsync(CancellationToken cancellationToken = default)
    {
        var tiers = await _repository.GetAllTiersAsync(cancellationToken);
        return tiers.Select(MapToDto).ToList();
    }

    public async Task<RewardTierDto> GetUserTierAsync(string userId, CancellationToken cancellationToken = default)
    {
        var totalPoints = await _repository.GetUserTotalPointsAsync(userId, cancellationToken);
        var tier = await _repository.GetTierByPointsAsync(totalPoints, cancellationToken);

        if (tier == null)
        {
            // Default to Bronze tier
            tier = new RewardTier
            {
                Name = "Bronze",
                MinimumPoints = 0,
                MaximumPoints = 499,
                BonusMultiplier = 1.0m,
                Benefits = "Basic rewards",
                IsActive = true
            };
        }

        return MapToDto(tier);
    }

    private RewardPointDto MapToDto(RewardPoint point) => new()
    {
        Id = point.Id,
        UserId = point.UserId,
        Points = point.Points,
        TransactionType = point.TransactionType,
        Description = point.Description,
        OrderId = point.OrderId,
        TransactionDate = point.TransactionDate
    };

    private RewardTierDto MapToDto(RewardTier tier) => new()
    {
        Id = tier.Id,
        Name = tier.Name,
        MinimumPoints = tier.MinimumPoints,
        MaximumPoints = tier.MaximumPoints,
        BonusMultiplier = tier.BonusMultiplier,
        Benefits = tier.Benefits,
        IsActive = tier.IsActive
    };
}
