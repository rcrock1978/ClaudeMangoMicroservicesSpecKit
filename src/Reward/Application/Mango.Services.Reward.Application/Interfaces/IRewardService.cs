using Mango.Services.Reward.Application.DTOs;

namespace Mango.Services.Reward.Application.Interfaces;

public interface IRewardService
{
    Task<UserRewardsDto> GetUserRewardsAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> CalculatePointsAsync(decimal orderAmount, string userId, int orderId, CancellationToken cancellationToken = default);
    Task<bool> AddPointsAsync(string userId, int points, string description, int? orderId = null, decimal? orderAmount = null, CancellationToken cancellationToken = default);
    Task<bool> RedeemPointsAsync(string userId, int points, CancellationToken cancellationToken = default);
    Task<List<RewardTierDto>> GetAllTiersAsync(CancellationToken cancellationToken = default);
    Task<RewardTierDto> GetUserTierAsync(string userId, CancellationToken cancellationToken = default);
}
