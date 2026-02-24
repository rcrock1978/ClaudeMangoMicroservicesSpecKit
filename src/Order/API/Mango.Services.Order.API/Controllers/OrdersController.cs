using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.Order.Application.MediatR.Commands;
using Mango.Services.Order.Application.MediatR.Queries;
using Mango.Services.Order.Application.DTOs;

namespace Mango.Services.Order.API.Controllers;

/// <summary>
/// API controller for order operations.
/// Provides endpoints for creating, updating, and retrieving orders.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a single order by ID.
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details or 404 if not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var query = new GetOrderQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get user's orders with pagination.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <returns>Paginated list of user's orders</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(PaginatedOrderResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedOrderResponse>> GetUserOrders(
        string userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1)
            return BadRequest("Page number must be >= 1");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100");

        var query = new GetUserOrdersQuery(userId, pageNumber, pageSize);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Create a new order.
    /// </summary>
    /// <param name="request">Order creation request with cart items</param>
    /// <returns>Created order details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (request == null || !request.Items.Any())
            return BadRequest("Order must contain at least one item");

        try
        {
            var command = new CreateOrderCommand(request);
            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update order status (state transitions).
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated order details</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        try
        {
            var command = new UpdateOrderStatusCommand(id, request);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cancel an order.
    /// </summary>
    /// <param name="id">Order ID to cancel</param>
    /// <returns>Updated (cancelled) order details</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> CancelOrder(int id)
    {
        try
        {
            var request = new UpdateOrderStatusRequest { Status = OrderStatusEnum.Cancelled };
            var command = new UpdateOrderStatusCommand(id, request);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
