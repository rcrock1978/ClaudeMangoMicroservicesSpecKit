using MediatR;
using Mango.Services.Order.Application.DTOs;
using Mango.Services.Order.Application.Interfaces;
using OrderEntity = Mango.Services.Order.Domain.Entities.Order;
using OrderStatus = Mango.Services.Order.Domain.Entities.OrderStatus;

namespace Mango.Services.Order.Application.MediatR.Commands;

/// <summary>
/// Command to update order status (state transitions).
/// </summary>
public class UpdateOrderStatusCommand : BaseCommand<OrderDto>
{
    public int OrderId { get; set; }
    public UpdateOrderStatusRequest Request { get; set; } = new();

    public UpdateOrderStatusCommand(int orderId, UpdateOrderStatusRequest request)
    {
        OrderId = orderId;
        Request = request;
    }
}

/// <summary>
/// Handler for UpdateOrderStatusCommand.
/// Handles state machine transitions for orders.
/// </summary>
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IOrderRepository _repository;

    public UpdateOrderStatusCommandHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId)
            ?? throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        var targetStatus = (OrderStatus)request.Request.Status;

        // Execute state transition
        var success = targetStatus switch
        {
            OrderStatus.Cancelled => order.CancelOrder(),
            OrderStatus.Processing => order.ConfirmOrder(),
            OrderStatus.Shipped => request.Request.PaymentTransactionId != null 
                ? order.ProcessPayment(request.Request.PaymentTransactionId)
                : throw new InvalidOperationException("PaymentTransactionId is required for Shipped status"),
            OrderStatus.Delivered => order.RecordDelivery(),
            _ => throw new InvalidOperationException($"Invalid status transition to {targetStatus}")
        };

        if (!success)
            throw new InvalidOperationException($"Cannot transition from {order.OrderStatus} to {targetStatus}");

        await _repository.UpdateAsync(order);

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
