using Serilog;
using Mango.Services.Admin.Application.DTOs;
using Mango.Services.Admin.Application.MediatR.Queries;
using Mango.Services.Admin.Infrastructure.HttpClients;
using RevenueMetricsDto = Mango.Services.Admin.Application.DTOs.RevenueMetricsDto;
using DailyRevenueDto = Mango.Services.Admin.Application.DTOs.DailyRevenueDto;

namespace Mango.Services.Admin.Infrastructure.Services;

/// <summary>
/// Service for aggregating data from multiple microservices into KPI metrics.
/// </summary>
public class DataAggregationService : IDataAggregationService
{
    private readonly IProductServiceClient _productServiceClient;
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IRewardServiceClient _rewardServiceClient;
    private readonly ICouponServiceClient _couponServiceClient;
    private readonly ILogger _logger;

    public DataAggregationService(
        IProductServiceClient productServiceClient,
        IOrderServiceClient orderServiceClient,
        IPaymentServiceClient paymentServiceClient,
        IRewardServiceClient rewardServiceClient,
        ICouponServiceClient couponServiceClient,
        ILogger logger)
    {
        _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
        _orderServiceClient = orderServiceClient ?? throw new ArgumentNullException(nameof(orderServiceClient));
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _rewardServiceClient = rewardServiceClient ?? throw new ArgumentNullException(nameof(rewardServiceClient));
        _couponServiceClient = couponServiceClient ?? throw new ArgumentNullException(nameof(couponServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Aggregates all KPI metrics from across microservices.
    /// </summary>
    public async Task<DashboardKpiDto?> GetDashboardKpisAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool includeDetailedBreakdown = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Aggregating dashboard KPIs from {StartDate} to {EndDate}",
                startDate?.ToString("yyyy-MM-dd") ?? "earliest",
                endDate?.ToString("yyyy-MM-dd") ?? "latest");

            // Set default date range (last 30 days)
            var effectiveStartDate = startDate ?? DateTime.UtcNow.AddDays(-30);
            var effectiveEndDate = endDate ?? DateTime.UtcNow;

            // Fetch all metrics in parallel
            var revenueTask = GetRevenueMetricsAsync(effectiveStartDate, effectiveEndDate, cancellationToken);
            var productsTask = GetProductMetricsAsync(cancellationToken);
            var customersTask = GetCustomerMetricsAsync(cancellationToken);
            var couponsTask = GetCouponMetricsAsync(cancellationToken);

            await Task.WhenAll(revenueTask, productsTask, customersTask, couponsTask);

            var dashboard = new DashboardKpiDto
            {
                LastUpdatedAt = DateTime.UtcNow,
                Revenue = await revenueTask,
                Products = await productsTask,
                Customers = await customersTask,
                Coupons = await couponsTask
            };

            _logger.Information("Successfully aggregated dashboard KPIs");
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error aggregating dashboard KPIs");
            return null;
        }
    }

    /// <summary>
    /// Aggregates revenue metrics from payment service and order service.
    /// </summary>
    public async Task<RevenueMetricsDto?> GetRevenueMetricsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Aggregating revenue metrics from {StartDate} to {EndDate}", startDate, endDate);

            // Use provided dates or fallback to default (last 30 days)
            var effectiveStartDate = startDate ?? DateTime.UtcNow.AddDays(-30);
            var effectiveEndDate = endDate ?? DateTime.UtcNow;

            // Fetch from payment service
            var revenueMetrics = await _paymentServiceClient.GetRevenueMetricsAsync(effectiveStartDate, effectiveEndDate);
            var dailyRevenue = await _paymentServiceClient.GetDailyRevenueAsync(effectiveStartDate, effectiveEndDate);
            var orderStats = await _orderServiceClient.GetOrderStatsAsync();

            if (revenueMetrics == null)
            {
                _logger.Warning("Failed to fetch revenue metrics from payment service");
                return null;
            }

            // Calculate growth percentage
            var previousStartDate = effectiveStartDate.AddDays(-(int)(effectiveEndDate - effectiveStartDate).TotalDays);
            var previousRevenueMetrics = await _paymentServiceClient.GetRevenueMetricsAsync(previousStartDate, effectiveStartDate);
            var growthPercentage = previousRevenueMetrics?.TotalRevenue > 0
                ? ((revenueMetrics.TotalRevenue - previousRevenueMetrics.TotalRevenue) / previousRevenueMetrics.TotalRevenue) * 100
                : 0m;

            return new RevenueMetricsDto
            {
                TotalRevenue = revenueMetrics.TotalRevenue,
                OrderCount = orderStats?.TotalOrders ?? 0,
                AverageOrderValue = revenueMetrics.AverageTransactionValue,
                GrowthPercentage = growthPercentage,
                DailyRevenue = dailyRevenue?.Select(x => new DailyRevenueDto
                {
                    Date = x.Date,
                    Amount = x.Revenue,
                    OrderCount = x.OrderCount
                }).ToList() ?? new(),
                MonthlyRevenue = new() // Can be calculated from daily data
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error aggregating revenue metrics");
            return null;
        }
    }

    /// <summary>
    /// Aggregates product metrics from product service.
    /// </summary>
    public async Task<ProductMetricsDto?> GetProductMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Aggregating product metrics");

            var productCount = await _productServiceClient.GetTotalProductCountAsync();
            var topProducts = await _productServiceClient.GetTopSellingProductsAsync(10);
            var lowStockProducts = await _productServiceClient.GetLowStockProductsAsync(10);
            var categories = await _productServiceClient.GetAllCategoriesAsync();

            var activeProducts = (await _productServiceClient.GetAllProductsAsync(1, 1000))
                ?.Count(x => x.IsActive) ?? 0;

            return new ProductMetricsDto
            {
                TotalProducts = productCount,
                TotalCategories = categories?.Count ?? 0,
                ActiveProducts = activeProducts,
                TopProducts = topProducts?.Select(p => new TopProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Revenue = p.Price * (p.Sales ?? 0),
                    UnitsSold = p.Sales ?? 0
                }).ToList() ?? new(),
                LowStockProducts = lowStockProducts?.Select(p => new LowStockProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CurrentStock = p.StockQuantity ?? 0,
                    ReorderLevel = 10 // Default reorder level
                }).ToList() ?? new()
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error aggregating product metrics");
            return null;
        }
    }

    /// <summary>
    /// Aggregates customer metrics from order and reward services.
    /// </summary>
    public async Task<CustomerMetricsDto?> GetCustomerMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Aggregating customer metrics");

            var rewardSummary = await _rewardServiceClient.GetRewardSummaryAsync();
            var tierDistribution = await _rewardServiceClient.GetCustomerTierDistributionAsync();
            var engagementMetrics = await _rewardServiceClient.GetRewardEngagementAsync();
            var orderStats = await _orderServiceClient.GetOrderStatsAsync();

            return new CustomerMetricsDto
            {
                ActiveCustomers = rewardSummary?.TotalCustomersEnrolled ?? 0,
                NewCustomersThisMonth = 0, // Would need additional data
                RepeatCustomers = rewardSummary?.ActiveParticipants ?? 0,
                AverageCustomerLifetimeValue = CalculateAverageLTV(orderStats, rewardSummary?.TotalCustomersEnrolled ?? 1),
                RetentionRate = engagementMetrics?.EngagementRate ?? 0,
                RewardEngagementRate = engagementMetrics?.EngagementRate ?? 0,
                AtRiskCustomers = 0, // Would need specific tracking
                DormantCustomers = 0 // Would need specific tracking
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error aggregating customer metrics");
            return null;
        }
    }

    /// <summary>
    /// Aggregates coupon metrics from coupon service.
    /// </summary>
    public async Task<CouponMetricsDto?> GetCouponMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Aggregating coupon metrics");

            var analyticsSummary = await _couponServiceClient.GetCouponAnalyticsSummaryAsync();
            var topCoupons = await _couponServiceClient.GetTopCouponsAsync(10);
            var expiringCoupons = await _couponServiceClient.GetExpiringCouponsAsync(7);

            return new CouponMetricsDto
            {
                ActiveCouponCount = analyticsSummary?.TotalActiveCoupons ?? 0,
                TotalCouponCount = analyticsSummary?.TotalCoupons ?? 0,
                TotalDiscountGiven = analyticsSummary?.TotalDiscountGiven ?? 0,
                AverageDiscountPercentage = analyticsSummary?.AverageDiscountPerCoupon ?? 0,
                MostUsedCoupons = topCoupons?.Select(c => new CouponUsageDto
                {
                    CouponCode = c.CouponCode,
                    TimesUsed = c.UsageCount,
                    TotalDiscountGiven = c.TotalDiscountGiven,
                    AverageDiscount = c.DiscountValue,
                    ExpiresAt = DateTime.UtcNow // Placeholder - would need actual expiry date
                }).ToList() ?? new(),
                RedemptionRate = analyticsSummary?.OverallRedemptionRate ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error aggregating coupon metrics");
            return null;
        }
    }

    /// <summary>
    /// Helper method to calculate average customer lifetime value.
    /// </summary>
    private decimal CalculateAverageLTV(OrderStatsDto? orderStats, int customerCount)
    {
        if (orderStats == null || customerCount <= 0)
            return 0;

        return orderStats.TotalRevenue / customerCount;
    }
}
