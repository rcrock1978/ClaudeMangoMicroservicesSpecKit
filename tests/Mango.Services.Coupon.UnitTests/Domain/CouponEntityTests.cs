using Xunit;
using FluentAssertions;
using CouponEntity = Mango.Services.Coupon.Domain.Entities.Coupon;

namespace Mango.Services.Coupon.UnitTests.Domain;

public class CouponEntityTests
{
    [Fact]
    public void CreateCoupon_WithValidData_ShouldSucceed()
    {
        var coupon = new CouponEntity
        {
            Code = "SAVE10",
            DiscountType = "Percentage",
            DiscountValue = 10,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            IsActive = true
        };

        coupon.Code.Should().Be("SAVE10");
        coupon.DiscountValue.Should().Be(10);
    }

    [Fact]
    public void CalculateDiscount_WithPercentage_ShouldCalculateCorrectly()
    {
        var coupon = new CouponEntity
        {
            DiscountType = "Percentage",
            DiscountValue = 10,
            MinimumCartValue = 0
        };

        var discount = coupon.CalculateDiscount(100);
        discount.Should().Be(10);
    }

    [Fact]
    public void CalculateDiscount_WithFixed_ShouldReturnFixedAmount()
    {
        var coupon = new CouponEntity
        {
            DiscountType = "Fixed",
            DiscountValue = 50,
            MinimumCartValue = 0
        };

        var discount = coupon.CalculateDiscount(100);
        discount.Should().Be(50);
    }

    [Fact]
    public void CalculateDiscount_WithMinimumCartValue_ShouldRespectMinimum()
    {
        var coupon = new CouponEntity
        {
            DiscountType = "Percentage",
            DiscountValue = 10,
            MinimumCartValue = 100
        };

        var discount = coupon.CalculateDiscount(50);
        discount.Should().Be(0);
    }

    [Fact]
    public void CalculateDiscount_WithMaximumDiscountCap_ShouldRespectCap()
    {
        var coupon = new CouponEntity
        {
            DiscountType = "Percentage",
            DiscountValue = 50,
            MaximumDiscountAmount = 20,
            MinimumCartValue = 0
        };

        var discount = coupon.CalculateDiscount(100);
        discount.Should().Be(20);
    }

    [Fact]
    public void IsValidAtTime_WithinDateRange_ShouldReturnTrue()
    {
        var now = DateTime.UtcNow;
        var coupon = new CouponEntity
        {
            IsActive = true,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(1)
        };

        coupon.IsValidAtTime(now).Should().BeTrue();
    }

    [Fact]
    public void IsValidAtTime_BeforeStartDate_ShouldReturnFalse()
    {
        var now = DateTime.UtcNow;
        var coupon = new CouponEntity
        {
            IsActive = true,
            StartDate = now.AddDays(1),
            EndDate = now.AddDays(2)
        };

        coupon.IsValidAtTime(now).Should().BeFalse();
    }

    [Fact]
    public void IsValidAtTime_InactiveCode_ShouldReturnFalse()
    {
        var coupon = new CouponEntity
        {
            IsActive = false,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1)
        };

        coupon.IsValidAtTime(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void HasReachedUsageLimit_WithinLimit_ShouldReturnFalse()
    {
        var coupon = new CouponEntity
        {
            MaxUsageCount = 10,
            CurrentUsageCount = 5
        };

        coupon.HasReachedUsageLimit().Should().BeFalse();
    }

    [Fact]
    public void HasReachedUsageLimit_AtLimit_ShouldReturnTrue()
    {
        var coupon = new CouponEntity
        {
            MaxUsageCount = 10,
            CurrentUsageCount = 10
        };

        coupon.HasReachedUsageLimit().Should().BeTrue();
    }

    [Fact]
    public void HasReachedUsageLimit_UnlimitedUsage_ShouldReturnFalse()
    {
        var coupon = new CouponEntity
        {
            MaxUsageCount = 0,
            CurrentUsageCount = 1000
        };

        coupon.HasReachedUsageLimit().Should().BeFalse();
    }

    [Fact]
    public void IncrementUsage_ShouldIncrementCount()
    {
        var coupon = new CouponEntity
        {
            MaxUsageCount = 10,
            CurrentUsageCount = 5
        };

        coupon.IncrementUsage();
        coupon.CurrentUsageCount.Should().Be(6);
    }

    [Fact]
    public void ValidateForApplication_WithValidCoupon_ShouldReturnValid()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            MaxUsageCount = 10,
            CurrentUsageCount = 5,
            MinimumCartValue = 0
        };

        var (isValid, _) = coupon.ValidateForApplication(100, 0);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateForApplication_WithInactiveCode_ShouldReturnInvalid()
    {
        var coupon = new CouponEntity
        {
            IsActive = false,
            MinimumCartValue = 0
        };

        var (isValid, message) = coupon.ValidateForApplication(100, 0);
        isValid.Should().BeFalse();
        message.Should().Contain("not active");
    }

    [Fact]
    public void ValidateForApplication_BelowMinimumCart_ShouldReturnInvalid()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            MinimumCartValue = 50
        };

        var (isValid, message) = coupon.ValidateForApplication(30, 0);
        isValid.Should().BeFalse();
        message.Should().Contain("Cart total must be");
    }
}
