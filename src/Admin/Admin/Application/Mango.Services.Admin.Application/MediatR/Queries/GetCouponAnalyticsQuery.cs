using MediatR;
using Mango.Services.Admin.Application.DTOs;

namespace Mango.Services.Admin.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve coupon analytics.
/// </summary>
public class GetCouponAnalyticsQuery : IRequest<CouponMetricsDto?>
{
}

/// <summary>
/// Handler for GetCouponAnalyticsQuery.
/// </summary>
public class GetCouponAnalyticsQueryHandler : IRequestHandler<GetCouponAnalyticsQuery, CouponMetricsDto?>
{
    private readonly IDataAggregationService _dataAggregationService;

    public GetCouponAnalyticsQueryHandler(IDataAggregationService dataAggregationService)
    {
        _dataAggregationService = dataAggregationService ?? throw new ArgumentNullException(nameof(dataAggregationService));
    }

    public async Task<CouponMetricsDto?> Handle(GetCouponAnalyticsQuery request, CancellationToken cancellationToken)
    {
        return await _dataAggregationService.GetCouponMetricsAsync(cancellationToken);
    }
}
