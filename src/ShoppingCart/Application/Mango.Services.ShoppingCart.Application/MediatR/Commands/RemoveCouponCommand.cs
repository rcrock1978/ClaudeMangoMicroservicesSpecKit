using MediatR;
using Mango.Services.ShoppingCart.Application.DTOs;
using Mango.Services.ShoppingCart.Application.Interfaces;

namespace Mango.Services.ShoppingCart.Application.MediatR.Commands;

/// <summary>
/// Command to remove the applied coupon from the shopping cart.
/// </summary>
public class RemoveCouponCommand : IRequest<ShoppingCartDto>
{
    public string UserId { get; set; }

    public RemoveCouponCommand(string userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Handler for RemoveCouponCommand.
/// </summary>
public class RemoveCouponCommandHandler : IRequestHandler<RemoveCouponCommand, ShoppingCartDto>
{
    private readonly IShoppingCartRepository _repository;

    public RemoveCouponCommandHandler(IShoppingCartRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShoppingCartDto> Handle(RemoveCouponCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdAsync(request.UserId)
            ?? throw new InvalidOperationException($"Cart not found for user {request.UserId}");

        cart.RemoveCoupon();
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
