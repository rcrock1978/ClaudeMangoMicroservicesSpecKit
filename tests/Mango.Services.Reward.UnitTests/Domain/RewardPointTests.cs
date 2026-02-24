using FluentAssertions;
using Mango.Services.Reward.Domain.Entities;

namespace Mango.Services.Reward.UnitTests.Domain;

public class RewardPointTests
{
    [Fact]
    public void IsValid_WithValidData_ShouldReturnTrue()
    {
        var point = new RewardPoint
        {
            UserId = "user123",
            Points = 100,
            TransactionType = "Earned",
            Description = "Order purchase"
        };

        point.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithMissingUserId_ShouldReturnFalse()
    {
        var point = new RewardPoint
        {
            UserId = string.Empty,
            Points = 100,
            TransactionType = "Earned"
        };

        point.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithZeroPoints_ShouldReturnFalse()
    {
        var point = new RewardPoint
        {
            UserId = "user123",
            Points = 0,
            TransactionType = "Earned"
        };

        point.IsValid().Should().BeFalse();
    }

    [Fact]
    public void RewardPoint_CanTrackOrderAssociation()
    {
        var point = new RewardPoint
        {
            UserId = "user123",
            Points = 50,
            TransactionType = "Earned",
            OrderId = 456,
            OrderAmount = 50.00m
        };

        point.OrderId.Should().Be(456);
        point.OrderAmount.Should().Be(50.00m);
    }
}
