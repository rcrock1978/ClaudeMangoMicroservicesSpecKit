using MediatR;
using Mango.Services.ShoppingCart.Application.DTOs;
using Mango.Services.ShoppingCart.Application.Interfaces;

namespace Mango.Services.ShoppingCart.Application.MediatR.Queries;

/// <summary>
/// Query to retrieve the shopping cart for a specific user.
/// </summary>
public class GetCartQuery : IRequest<ShoppingCartDto?>
{
    public string UserId { get; set; }

    public GetCartQuery(string userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Handler for GetCartQuery.
/// </summary>
public class GetCartQueryHandler : IRequestHandler<GetCartQuery, ShoppingCartDto?>
{
    private readonly IShoppingCartRepository _repository;

    public GetCartQueryHandler(IShoppingCartRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShoppingCartDto?> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdAsync(request.UserId);

        if (cart == null)
            return null;

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
