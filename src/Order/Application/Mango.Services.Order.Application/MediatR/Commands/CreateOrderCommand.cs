using MediatR;
using Mango.Services.Order.Application.DTOs;
using Mango.Services.Order.Application.Interfaces;
using OrderEntity = Mango.Services.Order.Domain.Entities.Order;

namespace Mango.Services.Order.Application.MediatR.Commands;

/// <summary>
/// Command to create a new order from cart items.
/// </summary>
public class CreateOrderCommand : BaseCommand<OrderDto>
{
    public CreateOrderRequest Request { get; set; } = new();

    public CreateOrderCommand(CreateOrderRequest request)
    {
        Request = request;
    }
}

/// <summary>
/// Handler for CreateOrderCommand.
/// Validates input, creates order, and calculates totals.
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate request
        if (request.Request == null || !request.Request.Items.Any())
            throw new InvalidOperationException("Order must contain at least one item");

        if (string.IsNullOrWhiteSpace(request.Request.UserId))
            throw new InvalidOperationException("UserId is required");

        if (string.IsNullOrWhiteSpace(request.Request.ShippingAddress))
            throw new InvalidOperationException("ShippingAddress is required");

        if (string.IsNullOrWhiteSpace(request.Request.PaymentMethod))
            throw new InvalidOperationException("PaymentMethod is required");

        // Create order
        var order = new OrderEntity
        {
            UserId = request.Request.UserId,
            OrderNumber = GenerateOrderNumber(),
            ShippingAddress = request.Request.ShippingAddress,
            BillingAddress = request.Request.BillingAddress,
            PaymentMethod = request.Request.PaymentMethod,
            DiscountAmount = request.Request.DiscountAmount,
            ShippingCost = request.Request.ShippingCost,
            Tax = request.Request.Tax
        };

        // Add items to order
        foreach (var item in request.Request.Items)
        {
            var addedSuccessfully = order.AddItem(
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.Quantity
            );

            if (!addedSuccessfully)
                throw new InvalidOperationException($"Failed to add item {item.ProductName} to order");
        }

        // Calculate and validate totals
        order.CalculateTotal();

        // Validate order before saving
        var validationErrors = order.GetValidationErrors();
        if (validationErrors.Any())
            throw new InvalidOperationException($"Order validation failed: {string.Join(", ", validationErrors)}");

        // Save order
        await _repository.AddAsync(order);

        return MapOrderDto(order);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
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
