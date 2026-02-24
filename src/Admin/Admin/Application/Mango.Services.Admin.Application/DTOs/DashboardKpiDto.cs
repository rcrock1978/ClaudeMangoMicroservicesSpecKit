namespace Mango.Services.Admin.Application.DTOs;

/// <summary>
/// Dashboard KPI metrics aggregated from multiple services.
/// Provides executive summary of business metrics.
/// </summary>
public class DashboardKpiDto
{
    /// <summary>
    /// Revenue metrics and trends.
    /// </summary>
    public RevenueMetricsDto Revenue { get; set; } = new();

    /// <summary>
    /// Product and inventory metrics.
    /// </summary>
    public ProductMetricsDto Products { get; set; } = new();

    /// <summary>
    /// Customer insights and analytics.
    /// </summary>
    public CustomerMetricsDto Customers { get; set; } = new();

    /// <summary>
    /// Coupon usage and effectiveness metrics.
    /// </summary>
    public CouponMetricsDto Coupons { get; set; } = new();

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Revenue metrics including daily/monthly trends.
/// </summary>
public class RevenueMetricsDto
{
    /// <summary>
    /// Total revenue for all time.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Total number of orders.
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// Average order value.
    /// </summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>
    /// Daily revenue trend for the last 30 days.
    /// </summary>
    public List<DailyRevenueDto> DailyRevenue { get; set; } = new();

    /// <summary>
    /// Monthly revenue trend for the last 12 months.
    /// </summary>
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();

    /// <summary>
    /// Revenue growth percentage compared to previous period.
    /// </summary>
    public decimal GrowthPercentage { get; set; }
}

/// <summary>
/// Daily revenue breakdown.
/// </summary>
public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>
/// Monthly revenue breakdown.
/// </summary>
public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>
/// Product and category metrics.
/// </summary>
public class ProductMetricsDto
{
    /// <summary>
    /// Total number of products.
    /// </summary>
    public int TotalProducts { get; set; }

    /// <summary>
    /// Total number of categories.
    /// </summary>
    public int TotalCategories { get; set; }

    /// <summary>
    /// Number of active (available) products.
    /// </summary>
    public int ActiveProducts { get; set; }

    /// <summary>
    /// Top performing products by revenue.
    /// </summary>
    public List<TopProductDto> TopProducts { get; set; } = new();

    /// <summary>
    /// Performance by category.
    /// </summary>
    public List<CategoryPerformanceDto> CategoryPerformance { get; set; } = new();

    /// <summary>
    /// Products with low stock.
    /// </summary>
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
}

/// <summary>
/// Top product by revenue.
/// </summary>
public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int UnitsSold { get; set; }
}

/// <summary>
/// Category performance metrics.
/// </summary>
public class CategoryPerformanceDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int ProductCount { get; set; }
    public int UnitsSold { get; set; }
}

/// <summary>
/// Product with low inventory.
/// </summary>
public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
}

/// <summary>
/// Customer insights and segmentation.
/// </summary>
public class CustomerMetricsDto
{
    /// <summary>
    /// Total active customers.
    /// </summary>
    public int ActiveCustomers { get; set; }

    /// <summary>
    /// New customers this month.
    /// </summary>
    public int NewCustomersThisMonth { get; set; }

    /// <summary>
    /// Repeat customers (purchased more than once).
    /// </summary>
    public int RepeatCustomers { get; set; }

    /// <summary>
    /// Average customer lifetime value.
    /// </summary>
    public decimal AverageCustomerLifetimeValue { get; set; }

    /// <summary>
    /// Customer retention rate (percentage).
    /// </summary>
    public decimal RetentionRate { get; set; }

    /// <summary>
    /// Reward points engagement rate.
    /// </summary>
    public decimal RewardEngagementRate { get; set; }

    /// <summary>
    /// At-risk customers (no purchases in 90+ days).
    /// </summary>
    public int AtRiskCustomers { get; set; }

    /// <summary>
    /// Dormant customers (no purchases in 180+ days).
    /// </summary>
    public int DormantCustomers { get; set; }
}

/// <summary>
/// Coupon usage and effectiveness metrics.
/// </summary>
public class CouponMetricsDto
{
    /// <summary>
    /// Total number of active coupons.
    /// </summary>
    public int ActiveCouponCount { get; set; }

    /// <summary>
    /// Total coupons created (all time).
    /// </summary>
    public int TotalCouponCount { get; set; }

    /// <summary>
    /// Total discount amount given via coupons.
    /// </summary>
    public decimal TotalDiscountGiven { get; set; }

    /// <summary>
    /// Average discount percentage.
    /// </summary>
    public decimal AverageDiscountPercentage { get; set; }

    /// <summary>
    /// Most used coupon codes.
    /// </summary>
    public List<CouponUsageDto> MostUsedCoupons { get; set; } = new();

    /// <summary>
    /// Overall coupon redemption rate.
    /// </summary>
    public decimal RedemptionRate { get; set; }
}

/// <summary>
/// Coupon usage statistics.
/// </summary>
public class CouponUsageDto
{
    public string CouponCode { get; set; } = string.Empty;
    public int TimesUsed { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public decimal AverageDiscount { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Standard response wrapper for API responses.
/// </summary>
public class ResponseDto
{
    public bool IsSuccess { get; set; } = true;
    public object? Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? ErrorCode { get; set; }
}

/// <summary>
/// Generic standard response wrapper.
/// </summary>
public class ResponseDto<T>
{
    public bool IsSuccess { get; set; } = true;
    public T? Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? ErrorCode { get; set; }
}
