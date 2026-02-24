using MediatR;
using Mango.Services.ShoppingCart.Application.DTOs;
using Mango.Services.ShoppingCart.Application.Interfaces;

namespace Mango.Services.ShoppingCart.Application.MediatR.Commands;

/// <summary>
/// Command to remove an item from the shopping cart.
/// </summary>
public class RemoveFromCartCommand : IRequest<ShoppingCartDto>
{
    public string UserId { get; set; }
    public int ProductId { get; set; }

    public RemoveFromCartCommand(string userId, int productId)
    {
        UserId = userId;
        ProductId = productId;
    }
}

/// <summary>
/// Handler for RemoveFromCartCommand.
/// </summary>
public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, ShoppingCartDto>
{
    private readonly IShoppingCartRepository _repository;

    public RemoveFromCartCommandHandler(IShoppingCartRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShoppingCartDto> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdAsync(request.UserId)
            ?? throw new InvalidOperationException($"Cart not found for user {request.UserId}");

        cart.RemoveItem(request.ProductId);
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
