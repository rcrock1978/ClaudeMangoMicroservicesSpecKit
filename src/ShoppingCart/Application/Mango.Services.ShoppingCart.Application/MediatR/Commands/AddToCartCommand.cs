using MediatR;
using Mango.Services.ShoppingCart.Application.DTOs;
using Mango.Services.ShoppingCart.Application.Interfaces;

namespace Mango.Services.ShoppingCart.Application.MediatR.Commands;

/// <summary>
/// Command to add an item to the shopping cart.
/// </summary>
public class AddToCartCommand : IRequest<ShoppingCartDto>
{
    public string UserId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }

    public AddToCartCommand(string userId, int productId, string productName, decimal productPrice,
        string? productImageUrl, int quantity)
    {
        UserId = userId;
        ProductId = productId;
        ProductName = productName;
        ProductPrice = productPrice;
        ProductImageUrl = productImageUrl;
        Quantity = quantity > 0 ? quantity : 1;
    }
}

/// <summary>
/// Handler for AddToCartCommand.
/// </summary>
public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, ShoppingCartDto>
{
    private readonly IShoppingCartRepository _repository;

    public AddToCartCommandHandler(IShoppingCartRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShoppingCartDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdAsync(request.UserId);

        if (cart == null)
        {
            // Create new cart for user
            cart = new Mango.Services.ShoppingCart.Domain.Entities.ShoppingCart
            {
                UserId = request.UserId
            };
            await _repository.AddAsync(cart);
        }

        // Add or merge item
        cart.AddItem(request.ProductId, request.ProductName, request.ProductPrice,
            request.ProductImageUrl, request.Quantity);
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
