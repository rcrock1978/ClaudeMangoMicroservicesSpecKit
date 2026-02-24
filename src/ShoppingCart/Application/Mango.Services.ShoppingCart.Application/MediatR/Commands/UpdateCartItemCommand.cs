using MediatR;
using Mango.Services.ShoppingCart.Application.DTOs;
using Mango.Services.ShoppingCart.Application.Interfaces;

namespace Mango.Services.ShoppingCart.Application.MediatR.Commands;

/// <summary>
/// Command to update the quantity of an item in the shopping cart.
/// </summary>
public class UpdateCartItemCommand : IRequest<ShoppingCartDto>
{
    public string UserId { get; set; }
    public int ProductId { get; set; }
    public int NewQuantity { get; set; }

    public UpdateCartItemCommand(string userId, int productId, int newQuantity)
    {
        UserId = userId;
        ProductId = productId;
        NewQuantity = newQuantity;
    }
}

/// <summary>
/// Handler for UpdateCartItemCommand.
/// </summary>
public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, ShoppingCartDto>
{
    private readonly IShoppingCartRepository _repository;

    public UpdateCartItemCommandHandler(IShoppingCartRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShoppingCartDto> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdAsync(request.UserId)
            ?? throw new InvalidOperationException($"Cart not found for user {request.UserId}");

        cart.UpdateItemQuantity(request.ProductId, request.NewQuantity);
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
