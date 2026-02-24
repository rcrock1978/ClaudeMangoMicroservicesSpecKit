using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.Coupon.Application.MediatR.Queries;
using Mango.Services.Coupon.Application.DTOs;

namespace Mango.Services.Coupon.API.Controllers;

/// <summary>
/// API controller for coupon operations.
/// Provides endpoints for retrieving and validating coupons.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CouponController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CouponController> _logger;

    public CouponController(IMediator mediator, ILogger<CouponController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get a coupon by its code.
    /// </summary>
    /// <param name="code">Coupon code</param>
    /// <returns>Coupon details or 404 if not found</returns>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponDto>> GetCouponByCode(string code)
    {
        _logger.LogInformation("Retrieving coupon with code: {CouponCode}", code);

        var query = new GetCouponByCodeQuery(code);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            _logger.LogWarning("Coupon not found with code: {CouponCode}", code);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Validate if a coupon can be applied to a cart.
    /// </summary>
    /// <param name="request">Coupon code and cart details</param>
    /// <returns>Validation result with discount information</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateCouponResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ValidateCouponResponse>> ValidateCoupon(ValidateCouponRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Code))
        {
            _logger.LogWarning("Invalid validate coupon request: code is empty or null");
            return BadRequest("Coupon code cannot be empty");
        }

        if (request.CartTotal < 0)
        {
            _logger.LogWarning("Invalid validate coupon request: cart total is negative");
            return BadRequest("Cart total cannot be negative");
        }

        _logger.LogInformation("Validating coupon: {CouponCode} for cart total: {CartTotal}",
            request.Code, request.CartTotal);

        var query = new ValidateCouponQuery(request.Code, request.CartTotal, request.UserUsageCount);
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
