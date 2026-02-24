namespace Mango.Services.Admin.Application.DTOs;

/// <summary>Daily revenue breakdown DTO.</summary>
public class DailyRevenueBreakdownDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>Tier breakdown for customer metrics.</summary>
public class TierBreakdownDto
{
    public int BronzeTierCount { get; set; }
    public int SilverTierCount { get; set; }
    public int GoldTierCount { get; set; }
    public int PlatinumTierCount { get; set; }
}

/// <summary>Most used coupon DTO.</summary>
public class MostUsedCouponDto
{
    public string Code { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal DiscountAmount { get; set; }
}
