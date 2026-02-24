using Mango.Services.Reward.Domain.Entities;

namespace Mango.Services.Reward.Application.Interfaces;

public interface IRewardRepository
{
    // RewardPoint operations
    Task AddRewardPointAsync(RewardPoint rewardPoint, CancellationToken cancellationToken = default);
    Task<int> GetUserTotalPointsAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<RewardPoint>> GetUserRewardPointsAsync(string userId, int take = 10, CancellationToken cancellationToken = default);

    // RewardTier operations
    Task<List<RewardTier>> GetAllTiersAsync(CancellationToken cancellationToken = default);
    Task<RewardTier?> GetTierByPointsAsync(int points, CancellationToken cancellationToken = default);
    Task<RewardTier?> GetTierAsync(int id, CancellationToken cancellationToken = default);
}
