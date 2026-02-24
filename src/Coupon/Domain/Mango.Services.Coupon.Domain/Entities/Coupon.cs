namespace Mango.Services.Coupon.Domain.Entities;

/// <summary>
/// Represents a discount coupon in the system.
/// </summary>
public class Coupon : BaseEntity
{
    /// <summary>
    /// Unique coupon code (e.g., "SAVE10").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the coupon.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Discount type: "Percentage" or "Fixed".
    /// </summary>
    public string DiscountType { get; set; } = "Percentage"; // "Percentage" or "Fixed"

    /// <summary>
    /// Discount value (percentage or fixed amount).
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Minimum cart total required to apply coupon.
    /// </summary>
    public decimal MinimumCartValue { get; set; }

    /// <summary>
    /// Maximum discount amount (for percentage-based coupons).
    /// </summary>
    public decimal? MaximumDiscountAmount { get; set; }

    /// <summary>
    /// Coupon start date (UTC).
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Coupon end date (UTC).
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Maximum number of times this coupon can be used (0 = unlimited).
    /// </summary>
    public int MaxUsageCount { get; set; }

    /// <summary>
    /// Current usage count.
    /// </summary>
    public int CurrentUsageCount { get; set; }

    /// <summary>
    /// Maximum usage per user (0 = unlimited).
    /// </summary>
    public int MaxUsagePerUser { get; set; }

    /// <summary>
    /// Is the coupon currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Check if coupon is valid for redemption at the given time.
    /// </summary>
    public bool IsValidAtTime(DateTime dateTime)
    {
        if (!IsActive)
            return false;

        return dateTime >= StartDate && dateTime <= EndDate;
    }

    /// <summary>
    /// Check if coupon has reached usage limit.
    /// </summary>
    public bool HasReachedUsageLimit()
    {
        if (MaxUsageCount == 0)
            return false;

        return CurrentUsageCount >= MaxUsageCount;
    }

    /// <summary>
    /// Calculate the discount amount for a given cart total.
    /// </summary>
    public decimal CalculateDiscount(decimal cartTotal)
    {
        if (cartTotal < MinimumCartValue)
            return 0;

        decimal discount;

        if (DiscountType == "Percentage")
        {
            discount = (cartTotal * DiscountValue) / 100;
            if (MaximumDiscountAmount.HasValue)
            {
                discount = Math.Min(discount, MaximumDiscountAmount.Value);
            }
        }
        else // Fixed
        {
            discount = DiscountValue;
        }

        // Discount cannot exceed cart total
        return Math.Min(discount, cartTotal);
    }

    /// <summary>
    /// Validate if coupon can be applied to a cart.
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateForApplication(decimal cartTotal, int userUsageCount)
    {
        if (!IsActive)
            return (false, "Coupon is not active");

        var now = DateTime.UtcNow;
        if (!IsValidAtTime(now))
            return (false, "Coupon is not valid at this time");

        if (HasReachedUsageLimit())
            return (false, "Coupon has reached its usage limit");

        if (MaxUsagePerUser > 0 && userUsageCount >= MaxUsagePerUser)
            return (false, "You have already used this coupon the maximum number of times");

        if (cartTotal < MinimumCartValue)
            return (false, $"Cart total must be at least {MinimumCartValue:C} to use this coupon");

        return (true, "");
    }

    /// <summary>
    /// Increment usage count after successful redemption.
    /// </summary>
    public void IncrementUsage()
    {
        if (!HasReachedUsageLimit())
        {
            CurrentUsageCount++;
        }
    }
}
