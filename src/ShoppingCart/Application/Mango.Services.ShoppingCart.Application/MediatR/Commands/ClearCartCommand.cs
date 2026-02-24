using MediatR;
using Mango.Services.ShoppingCart.Application.DTOs;
using Mango.Services.ShoppingCart.Application.Interfaces;

namespace Mango.Services.ShoppingCart.Application.MediatR.Commands;

/// <summary>
/// Command to clear all items from the shopping cart.
/// </summary>
public class ClearCartCommand : IRequest<ShoppingCartDto>
{
    public string UserId { get; set; }

    public ClearCartCommand(string userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Handler for ClearCartCommand.
/// </summary>
public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, ShoppingCartDto>
{
    private readonly IShoppingCartRepository _repository;

    public ClearCartCommandHandler(IShoppingCartRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShoppingCartDto> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdAsync(request.UserId)
            ?? throw new InvalidOperationException($"Cart not found for user {request.UserId}");

        cart.Clear();
        await _repository.UpdateAsync(cart);

        return MapCartDto(cart);
    }

    private static ShoppingCartDto MapCartDto(dynamic cart)
    {
        var items = ((List<dynamic>)cart.Items)
            .Select(item => new CartItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductPrice = item.ProductPrice,
                ProductImageUrl = item.ProductImageUrl,
                Quantity = item.Quantity
            }).ToList();

        return new ShoppingCartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = items,
            CouponCode = cart.CouponCode,
            DiscountAmount = cart.DiscountAmount,
            Subtotal = cart.GetSubtotal(),
            Total = cart.GetTotal(),
            ItemCount = cart.GetItemCount()
        };
    }
}
