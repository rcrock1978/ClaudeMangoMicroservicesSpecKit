using MediatR;
using Mango.Services.Order.Application.DTOs;
using Mango.Services.Order.Application.Interfaces;
using OrderEntity = Mango.Services.Order.Domain.Entities.Order;

namespace Mango.Services.Order.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve a single order by ID.
/// </summary>
public class GetOrderQuery : BaseQuery<OrderDto?>
{
    public int Id { get; set; }

    public GetOrderQuery(int id)
    {
        Id = id;
    }
}

/// <summary>
/// Handler for GetOrderQuery.
/// </summary>
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly IOrderRepository _repository;

    public GetOrderQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.Id);

        if (order == null)
            return null;

        return MapOrderDto(order);
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
