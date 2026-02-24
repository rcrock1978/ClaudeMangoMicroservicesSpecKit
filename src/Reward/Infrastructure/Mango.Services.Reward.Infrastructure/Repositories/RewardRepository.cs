using Mango.Services.Reward.Application.Interfaces;
using Mango.Services.Reward.Domain.Entities;
using Mango.Services.Reward.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.Reward.Infrastructure.Repositories;

public class RewardRepository : IRewardRepository
{
    private readonly RewardDbContext _context;

    public RewardRepository(RewardDbContext context)
    {
        _context = context;
    }

    public async Task AddRewardPointAsync(RewardPoint rewardPoint, CancellationToken cancellationToken = default)
    {
        _context.RewardPoints.Add(rewardPoint);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetUserTotalPointsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var earnedPoints = await _context.RewardPoints
            .Where(p => p.UserId == userId && p.TransactionType == "Earned")
            .SumAsync(p => p.Points, cancellationToken);

        var redeemedPoints = await _context.RewardPoints
            .Where(p => p.UserId == userId && p.TransactionType == "Redeemed")
            .SumAsync(p => p.Points, cancellationToken);

        return earnedPoints - redeemedPoints;
    }

    public async Task<List<RewardPoint>> GetUserRewardPointsAsync(string userId, int take = 10, CancellationToken cancellationToken = default)
    {
        return await _context.RewardPoints
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.TransactionDate)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RewardTier>> GetAllTiersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RewardTiers
            .Where(t => t.IsActive)
            .OrderBy(t => t.MinimumPoints)
            .ToListAsync(cancellationToken);
    }

    public async Task<RewardTier?> GetTierByPointsAsync(int points, CancellationToken cancellationToken = default)
    {
        return await _context.RewardTiers
            .Where(t => t.IsActive && points >= t.MinimumPoints && points <= t.MaximumPoints)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<RewardTier?> GetTierAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.RewardTiers.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
}
