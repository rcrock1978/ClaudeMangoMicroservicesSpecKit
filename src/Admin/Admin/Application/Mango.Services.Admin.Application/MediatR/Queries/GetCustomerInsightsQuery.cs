using MediatR;
using Mango.Services.Admin.Application.DTOs;

namespace Mango.Services.Admin.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve customer insights and analytics.
/// </summary>
public class GetCustomerInsightsQuery : IRequest<CustomerMetricsDto?>
{
}

/// <summary>
/// Handler for GetCustomerInsightsQuery.
/// </summary>
public class GetCustomerInsightsQueryHandler : IRequestHandler<GetCustomerInsightsQuery, CustomerMetricsDto?>
{
    private readonly IDataAggregationService _dataAggregationService;

    public GetCustomerInsightsQueryHandler(IDataAggregationService dataAggregationService)
    {
        _dataAggregationService = dataAggregationService ?? throw new ArgumentNullException(nameof(dataAggregationService));
    }

    public async Task<CustomerMetricsDto?> Handle(GetCustomerInsightsQuery request, CancellationToken cancellationToken)
    {
        return await _dataAggregationService.GetCustomerMetricsAsync(cancellationToken);
    }
}
