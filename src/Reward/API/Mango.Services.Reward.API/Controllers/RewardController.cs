using Mango.Services.Reward.Application.DTOs;
using Mango.Services.Reward.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.Reward.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RewardController : ControllerBase
{
    private readonly IRewardService _rewardService;
    private readonly ILogger<RewardController> _logger;

    public RewardController(IRewardService rewardService, ILogger<RewardController> logger)
    {
        _rewardService = rewardService;
        _logger = logger;
    }

    /// <summary>
    /// Get user rewards and points.
    /// GET /api/reward/{userId}
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserRewards(string userId, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(ResponseDto.Error("User ID is required", 400));

            var rewards = await _rewardService.GetUserRewardsAsync(userId, cancellationToken);
            return Ok(ResponseDto<UserRewardsDto>.Success(rewards, "User rewards retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rewards for user: {UserId}", userId);
            return StatusCode(500, ResponseDto.Error("An error occurred while retrieving rewards", 500));
        }
    }

    /// <summary>
    /// Get all reward tiers.
    /// GET /api/reward/tiers
    /// </summary>
    [HttpGet("tiers")]
    public async Task<IActionResult> GetRewardTiers(CancellationToken cancellationToken)
    {
        try
        {
            var tiers = await _rewardService.GetAllTiersAsync(cancellationToken);
            return Ok(ResponseDto<List<RewardTierDto>>.Success(tiers, "Reward tiers retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reward tiers");
            return StatusCode(500, ResponseDto.Error("An error occurred while retrieving reward tiers", 500));
        }
    }

    /// <summary>
    /// Redeem reward points for discount.
    /// POST /api/reward/redeem
    /// </summary>
    [HttpPost("redeem")]
    public async Task<IActionResult> RedeemPoints([FromBody] RedeemRewardRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || request.PointsToRedeem <= 0)
                return BadRequest(ResponseDto.Error("Invalid request data", 400));

            var success = await _rewardService.RedeemPointsAsync(request.UserId, request.PointsToRedeem, cancellationToken);

            if (!success)
                return BadRequest(ResponseDto.Error("Failed to redeem points. Insufficient balance.", 400));

            return Ok(ResponseDto.Success(null, "Points redeemed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming points for user: {UserId}", request.UserId);
            return StatusCode(500, ResponseDto.Error("An error occurred while redeeming points", 500));
        }
    }
}
