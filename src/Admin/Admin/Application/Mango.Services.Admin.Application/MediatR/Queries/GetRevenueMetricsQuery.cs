using MediatR;
using Mango.Services.Admin.Application.DTOs;

namespace Mango.Services.Admin.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve revenue metrics for a date range.
/// </summary>
public class GetRevenueMetricsQuery : IRequest<RevenueMetricsDto?>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Handler for GetRevenueMetricsQuery.
/// </summary>
public class GetRevenueMetricsQueryHandler : IRequestHandler<GetRevenueMetricsQuery, RevenueMetricsDto?>
{
    private readonly IDataAggregationService _dataAggregationService;

    public GetRevenueMetricsQueryHandler(IDataAggregationService dataAggregationService)
    {
        _dataAggregationService = dataAggregationService ?? throw new ArgumentNullException(nameof(dataAggregationService));
    }

    public async Task<RevenueMetricsDto?> Handle(GetRevenueMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _dataAggregationService.GetRevenueMetricsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}
