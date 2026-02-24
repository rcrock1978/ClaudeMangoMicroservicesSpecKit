using MediatR;
using Mango.Services.Admin.Application.DTOs;

namespace Mango.Services.Admin.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve product metrics.
/// </summary>
public class GetProductMetricsQuery : IRequest<ProductMetricsDto?>
{
}

/// <summary>
/// Handler for GetProductMetricsQuery.
/// </summary>
public class GetProductMetricsQueryHandler : IRequestHandler<GetProductMetricsQuery, ProductMetricsDto?>
{
    private readonly IDataAggregationService _dataAggregationService;

    public GetProductMetricsQueryHandler(IDataAggregationService dataAggregationService)
    {
        _dataAggregationService = dataAggregationService ?? throw new ArgumentNullException(nameof(dataAggregationService));
    }

    public async Task<ProductMetricsDto?> Handle(GetProductMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _dataAggregationService.GetProductMetricsAsync(cancellationToken);
    }
}
