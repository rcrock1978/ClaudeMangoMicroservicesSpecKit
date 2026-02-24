using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mango.Services.Admin.Application.DTOs;

namespace Mango.Services.Admin.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve dashboard KPI metrics.
/// Aggregates data from Product, Order, Payment, and Reward services.
/// </summary>
public class GetDashboardKpisQuery : BaseQuery<DashboardKpiDto>
{
    /// <summary>
    /// Optional date range for metrics (default: all time).
    /// </summary>
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Include detailed breakdowns (daily/monthly trends, categories, etc.)
    /// </summary>
    public bool IncludeDetailedBreakdown { get; set; } = false;
}

/// <summary>
/// Handler for GetDashboardKpisQuery.
/// Orchestrates calls to multiple microservices and aggregates results.
/// </summary>
public class GetDashboardKpisQueryHandler : IRequestHandler<GetDashboardKpisQuery, DashboardKpiDto>
{
    private readonly IDataAggregationService _aggregationService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetDashboardKpisQueryHandler> _logger;

    private const string DashboardCacheKey = "admin:dashboard:kpis";
    private const int CacheDurationMinutes = 5;

    public GetDashboardKpisQueryHandler(
        IDataAggregationService aggregationService,
        IMemoryCache cache,
        ILogger<GetDashboardKpisQueryHandler> logger)
    {
        _aggregationService = aggregationService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DashboardKpiDto> Handle(GetDashboardKpisQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = $"{DashboardCacheKey}:{request.IncludeDetailedBreakdown}";
            if (!request.StartDate.HasValue && !request.EndDate.HasValue &&
                _cache.TryGetValue(cacheKey, out DashboardKpiDto? cachedResult))
            {
                _logger.LogInformation("Dashboard KPIs retrieved from cache");
                return cachedResult!;
            }

            // Aggregate data from services
            _logger.LogInformation("Aggregating dashboard KPIs from services");

            var kpis = await _aggregationService.GetDashboardKpisAsync(
                request.StartDate,
                request.EndDate,
                request.IncludeDetailedBreakdown,
                cancellationToken);

            // Cache the result
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));

            _cache.Set(cacheKey, kpis, cacheOptions);

            _logger.LogInformation("Dashboard KPIs aggregated successfully");
            return kpis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard KPIs");
            throw;
        }
    }
}

/// <summary>
/// Service interface for aggregating data from multiple microservices.
/// </summary>
public interface IDataAggregationService
{
    /// <summary>
    /// Aggregates dashboard KPI metrics from all services.
    /// </summary>
    Task<DashboardKpiDto> GetDashboardKpisAsync(
        DateTime? startDate,
        DateTime? endDate,
        bool includeDetailedBreakdown,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets revenue metrics from Payment and Order services.
    /// </summary>
    Task<RevenueMetricsDto> GetRevenueMetricsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets product metrics from Product service.
    /// </summary>
    Task<ProductMetricsDto> GetProductMetricsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets customer metrics from Order and Reward services.
    /// </summary>
    Task<CustomerMetricsDto> GetCustomerMetricsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets coupon metrics from Coupon and Order services.
    /// </summary>
    Task<CouponMetricsDto> GetCouponMetricsAsync(CancellationToken cancellationToken);
}
