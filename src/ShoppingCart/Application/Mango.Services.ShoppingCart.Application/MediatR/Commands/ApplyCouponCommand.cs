using MediatR;
using Mango.Services.ShoppingCart.Application.DTOs;
using Mango.Services.ShoppingCart.Application.Interfaces;

namespace Mango.Services.ShoppingCart.Application.MediatR.Commands;

/// <summary>
/// Command to apply a coupon code to the shopping cart.
/// </summary>
public class ApplyCouponCommand : IRequest<ShoppingCartDto>
{
    public string UserId { get; set; }
    public string CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }

    public ApplyCouponCommand(string userId, string couponCode, decimal discountAmount)
    {
        UserId = userId;
        CouponCode = couponCode;
        DiscountAmount = discountAmount;
    }
}

/// <summary>
/// Handler for ApplyCouponCommand.
/// </summary>
public class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, ShoppingCartDto>
{
    private readonly IShoppingCartRepository _repository;

    public ApplyCouponCommandHandler(IShoppingCartRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShoppingCartDto> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdAsync(request.UserId)
            ?? throw new InvalidOperationException($"Cart not found for user {request.UserId}");

        cart.ApplyCoupon(request.CouponCode, request.DiscountAmount);
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
