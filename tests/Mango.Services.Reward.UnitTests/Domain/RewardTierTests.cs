using FluentAssertions;
using Mango.Services.Reward.Domain.Entities;

namespace Mango.Services.Reward.UnitTests.Domain;

public class RewardTierTests
{
    [Fact]
    public void IsValid_WithValidTier_ShouldReturnTrue()
    {
        var tier = new RewardTier
        {
            Name = "Gold",
            MinimumPoints = 1000,
            MaximumPoints = 4999,
            BonusMultiplier = 2.0m
        };

        tier.IsValid().Should().BeTrue();
    }

    [Fact]
    public void CreateDefaultTiers_ShouldReturnFourTiers()
    {
        var tiers = RewardTier.CreateDefaultTiers();

        tiers.Should().HaveCount(4);
        tiers.Select(t => t.Name).Should().Contain(new[] { "Bronze", "Silver", "Gold", "Platinum" });
    }

    [Fact]
    public void DefaultTiers_ShouldHaveCorrectMultipliers()
    {
        var tiers = RewardTier.CreateDefaultTiers();

        tiers.FirstOrDefault(t => t.Name == "Bronze")?.BonusMultiplier.Should().Be(1.0m);
        tiers.FirstOrDefault(t => t.Name == "Silver")?.BonusMultiplier.Should().Be(1.5m);
        tiers.FirstOrDefault(t => t.Name == "Gold")?.BonusMultiplier.Should().Be(2.0m);
        tiers.FirstOrDefault(t => t.Name == "Platinum")?.BonusMultiplier.Should().Be(3.0m);
    }

    [Fact]
    public void IsValid_WithInvalidRanges_ShouldReturnFalse()
    {
        var tier = new RewardTier
        {
            Name = "Invalid",
            MinimumPoints = 1000,
            MaximumPoints = 500, // Invalid: Max < Min
            BonusMultiplier = 1.5m
        };

        tier.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNegativeMultiplier_ShouldReturnFalse()
    {
        var tier = new RewardTier
        {
            Name = "Invalid",
            MinimumPoints = 0,
            MaximumPoints = 100,
            BonusMultiplier = -1.0m
        };

        tier.IsValid().Should().BeFalse();
    }
}
