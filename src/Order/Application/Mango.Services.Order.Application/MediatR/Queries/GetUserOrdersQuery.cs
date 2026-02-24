using MediatR;
using Mango.Services.Order.Application.DTOs;
using Mango.Services.Order.Application.Interfaces;
using OrderEntity = Mango.Services.Order.Domain.Entities.Order;

namespace Mango.Services.Order.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve user's orders with pagination.
/// </summary>
public class GetUserOrdersQuery : BaseQuery<PaginatedOrderResponse>
{
    public string UserId { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public GetUserOrdersQuery(string userId, int pageNumber = 1, int pageSize = 10)
    {
        UserId = userId;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}

/// <summary>
/// Handler for GetUserOrdersQuery.
/// </summary>
public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, PaginatedOrderResponse>
{
    private readonly IOrderRepository _repository;

    public GetUserOrdersQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedOrderResponse> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return new PaginatedOrderResponse();

        var (orders, totalCount) = await _repository.GetByUserIdAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize
        );

        return new PaginatedOrderResponse
        {
            Items = orders.Select(MapOrderDto).ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static OrderDto MapOrderDto(OrderEntity order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderNumber = order.OrderNumber,
            OrderStatus = (OrderStatusEnum)order.OrderStatus,
            OrderDate = order.OrderDate,
            DeliveryDate = order.DeliveryDate,
            ShippingAddress = order.ShippingAddress,
            BillingAddress = order.BillingAddress,
            PaymentMethod = order.PaymentMethod,
            PaymentTransactionId = order.PaymentTransactionId,
            DiscountAmount = order.DiscountAmount,
            ShippingCost = order.ShippingCost,
            Tax = order.Tax,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice,
                DiscountAmount = i.DiscountAmount
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}
