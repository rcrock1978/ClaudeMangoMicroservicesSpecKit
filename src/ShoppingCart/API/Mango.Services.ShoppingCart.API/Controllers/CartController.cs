using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.ShoppingCart.Application.MediatR.Commands;
using Mango.Services.ShoppingCart.Application.MediatR.Queries;
using Mango.Services.ShoppingCart.Application.DTOs;

namespace Mango.Services.ShoppingCart.API.Controllers;

/// <summary>
/// API controller for shopping cart operations.
/// Provides endpoints for managing user shopping carts and items.
/// </summary>
[ApiController]
[Route("api/cart")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the shopping cart for the current user.
    /// </summary>
    /// <param name="userId">User ID (typically from JWT token)</param>
    /// <returns>Shopping cart with items and totals, or 404 if empty</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingCartDto>> GetCart([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId is required");

        var query = new GetCartQuery(userId);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Add an item to the shopping cart.
    /// </summary>
    /// <param name="userId">User ID (typically from JWT token)</param>
    /// <param name="request">Item details to add</param>
    /// <returns>Updated shopping cart</returns>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShoppingCartDto>> AddToCart(
        [FromQuery] string userId,
        [FromBody] AddToCartRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId is required");

        if (request.ProductId <= 0)
            return BadRequest("ProductId must be greater than zero");

        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero");

        var command = new AddToCartCommand(
            userId,
            request.ProductId,
            request.ProductName,
            request.ProductPrice,
            request.ProductImageUrl,
            request.Quantity
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Update the quantity of an item in the shopping cart.
    /// </summary>
    /// <param name="userId">User ID (typically from JWT token)</param>
    /// <param name="productId">Product ID to update</param>
    /// <param name="request">New quantity (0 to remove item)</param>
    /// <returns>Updated shopping cart</returns>
    [HttpPut("items/{productId}")]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingCartDto>> UpdateCartItem(
        [FromQuery] string userId,
        int productId,
        [FromBody] UpdateCartItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId is required");

        if (productId <= 0)
            return BadRequest("ProductId must be greater than zero");

        var command = new UpdateCartItemCommand(userId, productId, request.Quantity);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Remove an item from the shopping cart.
    /// </summary>
    /// <param name="userId">User ID (typically from JWT token)</param>
    /// <param name="productId">Product ID to remove</param>
    /// <returns>Updated shopping cart</returns>
    [HttpDelete("items/{productId}")]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingCartDto>> RemoveFromCart(
        [FromQuery] string userId,
        int productId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId is required");

        if (productId <= 0)
            return BadRequest("ProductId must be greater than zero");

        var command = new RemoveFromCartCommand(userId, productId);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Clear all items from the shopping cart.
    /// </summary>
    /// <param name="userId">User ID (typically from JWT token)</param>
    /// <returns>Cleared shopping cart</returns>
    [HttpDelete]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingCartDto>> ClearCart([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId is required");

        var command = new ClearCartCommand(userId);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Apply a coupon code to the shopping cart.
    /// </summary>
    /// <param name="userId">User ID (typically from JWT token)</param>
    /// <param name="request">Coupon code and discount amount</param>
    /// <returns>Updated shopping cart with discount applied</returns>
    [HttpPost("coupon")]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingCartDto>> ApplyCoupon(
        [FromQuery] string userId,
        [FromBody] ApplyCouponRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId is required");

        if (string.IsNullOrWhiteSpace(request.CouponCode))
            return BadRequest("CouponCode is required");

        if (request.DiscountAmount < 0)
            return BadRequest("DiscountAmount cannot be negative");

        var command = new ApplyCouponCommand(userId, request.CouponCode, request.DiscountAmount);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Remove the applied coupon from the shopping cart.
    /// </summary>
    /// <param name="userId">User ID (typically from JWT token)</param>
    /// <returns>Updated shopping cart without discount</returns>
    [HttpDelete("coupon")]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingCartDto>> RemoveCoupon([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId is required");

        var command = new RemoveCouponCommand(userId);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
