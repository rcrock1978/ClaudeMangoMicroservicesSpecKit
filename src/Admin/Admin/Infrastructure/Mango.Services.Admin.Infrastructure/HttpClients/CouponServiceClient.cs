using Serilog;

namespace Mango.Services.Admin.Infrastructure.HttpClients;

/// <summary>
/// HTTP client for communicating with the Coupon microservice.
/// </summary>
public interface ICouponServiceClient
{
    /// <summary>Gets all coupons with pagination.</summary>
    Task<List<CouponDto>?> GetAllCouponsAsync(int pageNumber = 1, int pageSize = 50);

    /// <summary>Gets a single coupon by code.</summary>
    Task<CouponDto?> GetCouponByCodeAsync(string code);

    /// <summary>Gets total active coupons count.</summary>
    Task<int> GetActiveCouponCountAsync();

    /// <summary>Gets coupon usage statistics.</summary>
    Task<CouponUsageStatsDto?> GetCouponUsageStatsAsync(string code);

    /// <summary>Gets all coupon analytics summary.</summary>
    Task<CouponAnalyticsSummaryDto?> GetCouponAnalyticsSummaryAsync();

    /// <summary>Gets coupons expiring within specified days.</summary>
    Task<List<CouponDto>?> GetExpiringCouponsAsync(int daysFromNow = 7);

    /// <summary>Gets most used coupons.</summary>
    Task<List<CouponPerformanceDto>?> GetTopCouponsAsync(int count = 10);
}

public class CouponServiceClient : BaseServiceClient, ICouponServiceClient
{
    public CouponServiceClient(HttpClient httpClient, ILogger logger)
        : base(httpClient, logger)
    {
    }

    public async Task<List<CouponDto>?> GetAllCouponsAsync(int pageNumber = 1, int pageSize = 50)
    {
        _logger.Information("Fetching coupons: page {PageNumber}, size {PageSize}", pageNumber, pageSize);
        return await GetAsync<List<CouponDto>>($"/api/coupons?pageNumber={pageNumber}&pageSize={pageSize}");
    }

    public async Task<CouponDto?> GetCouponByCodeAsync(string code)
    {
        _logger.Information("Fetching coupon {Code}", code);
        return await GetAsync<CouponDto>($"/api/coupons/{code}");
    }

    public async Task<int> GetActiveCouponCountAsync()
    {
        _logger.Information("Fetching active coupon count");
        var result = await GetAsync<dynamic>("/api/coupons/count/active");
        return result?.count ?? 0;
    }

    public async Task<CouponUsageStatsDto?> GetCouponUsageStatsAsync(string code)
    {
        _logger.Information("Fetching usage stats for coupon {Code}", code);
        return await GetAsync<CouponUsageStatsDto>($"/api/coupons/{code}/stats");
    }

    public async Task<CouponAnalyticsSummaryDto?> GetCouponAnalyticsSummaryAsync()
    {
        _logger.Information("Fetching coupon analytics summary");
        return await GetAsync<CouponAnalyticsSummaryDto>("/api/coupons/analytics/summary");
    }

    public async Task<List<CouponDto>?> GetExpiringCouponsAsync(int daysFromNow = 7)
    {
        _logger.Information("Fetching coupons expiring in {Days} days", daysFromNow);
        return await GetAsync<List<CouponDto>>($"/api/coupons/expiring?days={daysFromNow}");
    }

    public async Task<List<CouponPerformanceDto>?> GetTopCouponsAsync(int count = 10)
    {
        _logger.Information("Fetching top {Count} coupons by usage", count);
        return await GetAsync<List<CouponPerformanceDto>>($"/api/coupons/top?count={count}");
    }
}

/// <summary>DTO for coupon data from Coupon service.</summary>
public class CouponDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty; // "Percentage" or "Fixed"
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscount { get; set; }
    public decimal? MinCartValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>DTO for coupon usage statistics.</summary>
public class CouponUsageStatsDto
{
    public string CouponCode { get; set; } = string.Empty;
    public int TotalUsageCount { get; set; }
    public int RemainingUsage { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public decimal AverageDiscount { get; set; }
    public int UniqueCustomersUsed { get; set; }
    public DateTime LastUsedDate { get; set; }
    public string Status { get; set; } = string.Empty; // "Active", "Expired", "Used Up"
    public decimal RedemptionRate { get; set; }
}

/// <summary>DTO for coupon analytics summary.</summary>
public class CouponAnalyticsSummaryDto
{
    public int TotalActiveCoupons { get; set; }
    public int TotalCoupons { get; set; }
    public int ExpiredCoupons { get; set; }
    public int CouponsUsedUp { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public decimal AverageDiscountPerCoupon { get; set; }
    public int TotalUniqueCouponsRedeemed { get; set; }
    public decimal OverallRedemptionRate { get; set; }
}

/// <summary>DTO for coupon performance metrics.</summary>
public class CouponPerformanceDto
{
    public string CouponCode { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public int UniqueCustomersUsed { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal RedemptionPercentage { get; set; }
}
