namespace Mango.Services.Payment.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.Payment.Application.DTOs;
using Mango.Services.Payment.Application.Interfaces;
using Mango.Services.Payment.Application.MediatR.Commands;
using Mango.Services.Payment.Application.MediatR.Queries;
using Mango.Services.Payment.Domain;

/// <summary>
/// API controller for payment operations.
/// Handles payment initiation, confirmation, refunds, and status queries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    /// <summary>
    /// Initializes a new instance of PaymentController.
    /// </summary>
    public PaymentController(
        IMediator mediator,
        IPaymentService paymentService,
        ILogger<PaymentController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initiate a new payment (creates payment intent/order).
    /// </summary>
    /// <param name="request">Payment initiation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created payment with transaction ID</returns>
    [HttpPost("initiate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiatePayment(
        [FromBody] PaymentInitiateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst("sub")?.Value ?? "system";
            var command = new InitiatePaymentCommand(request, userId);
            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Payment initiated for order {OrderId}", request.OrderId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid payment initiation request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Confirm and process a payment.
    /// </summary>
    /// <param name="request">Payment confirmation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated payment with final status</returns>
    [HttpPost("confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPayment(
        [FromBody] PaymentConfirmRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst("sub")?.Value ?? "system";
            var command = new ConfirmPaymentCommand(request, userId);
            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Payment {PaymentId} confirmed", request.PaymentId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid payment confirmation request");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Payment confirmation operation failed");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Refund a payment (fully or partially).
    /// </summary>
    /// <param name="request">Refund request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated payment with refund details</returns>
    [HttpPost("refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundPayment(
        [FromBody] RefundRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst("sub")?.Value ?? "system";
            var command = new RefundPaymentCommand(request, userId);
            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Payment {PaymentId} refunded", request.PaymentId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid refund request");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Refund operation failed");
            return StatusCode(StatusCodes.Status409Conflict, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get payment status for a specific payment.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current payment status and details</returns>
    [HttpGet("{paymentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentStatus(
        int paymentId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (paymentId <= 0)
                return BadRequest(new { error = "Invalid payment ID" });

            var userId = User.FindFirst("sub")?.Value ?? "system";
            var query = new GetPaymentStatusQuery(paymentId, userId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Payment not found: {PaymentId}", paymentId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment status");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get payment history for the current user.
    /// </summary>
    /// <param name="skip">Number of records to skip</param>
    /// <param name="take">Number of records to take</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's payments</returns>
    [HttpGet("user/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentHistory(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "system";

            if (skip < 0)
                skip = 0;

            if (take < 1 || take > 100)
                take = 10;

            var query = new GetPaymentHistoryQuery(userId, skip, take);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(new { data = result, count = result.Count, skip, take });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment history");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all payments for a specific order.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of payments for the order</returns>
    [HttpGet("order/{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderPayments(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (orderId <= 0)
                return BadRequest(new { error = "Invalid order ID" });

            var payments = await _paymentService.GetOrderPaymentsAsync(orderId, cancellationToken);
            return Ok(new { data = payments, count = payments.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order payments");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get list of available payment methods.
    /// </summary>
    /// <returns>List of supported payment methods</returns>
    [HttpGet("methods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentMethods()
    {
        try
        {
            var methods = await _paymentService.GetAvailablePaymentMethodsAsync();
            return Ok(new { data = methods });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment methods");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get list of supported currencies.
    /// </summary>
    /// <returns>List of supported currency codes</returns>
    [HttpGet("currencies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportedCurrencies()
    {
        try
        {
            var currencies = await _paymentService.GetSupportedCurrenciesAsync();
            return Ok(new { data = currencies });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported currencies");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Handle payment gateway webhooks (Stripe or PayPal).
    /// </summary>
    /// <param name="gateway">Payment gateway name (stripe or paypal)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Webhook processing result</returns>
    [HttpPost("webhook/{gateway}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandleWebhook(
        [FromRoute] string gateway,
        CancellationToken cancellationToken)
    {
        try
        {
            // Read the raw request body
            var body = string.Empty;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(body))
                return BadRequest(new { error = "Webhook body is empty" });

            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ??
                          Request.Headers["X-PAYPAL-TRANSMISSION-SIG"].FirstOrDefault() ??
                          string.Empty;

            if (string.IsNullOrEmpty(signature))
                return Unauthorized(new { error = "Missing webhook signature" });

            // Determine gateway
            var gatewayType = gateway.ToLower() switch
            {
                "stripe" => PaymentGateway.Stripe,
                "paypal" => PaymentGateway.PayPal,
                _ => throw new ArgumentException($"Unknown payment gateway: {gateway}")
            };

            var success = await _paymentService.ProcessWebhookAsync(gatewayType, body, signature, cancellationToken);

            if (!success)
                return Unauthorized(new { error = "Webhook signature verification failed" });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook from {Gateway}", gateway);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Cancel a pending payment.
    /// </summary>
    /// <param name="paymentId">Payment ID to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated payment</returns>
    [HttpPost("{paymentId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPayment(
        int paymentId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (paymentId <= 0)
                return BadRequest(new { error = "Invalid payment ID" });

            var result = await _paymentService.CancelPaymentAsync(paymentId, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel payment {PaymentId}", paymentId);
            return StatusCode(StatusCodes.Status409Conflict, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Validate a payment amount and currency combination.
    /// </summary>
    /// <param name="amount">Amount to validate</param>
    /// <param name="currency">Currency code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation errors if any</returns>
    [HttpGet("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidatePaymentAmount(
        [FromQuery] decimal amount,
        [FromQuery] string currency = "USD",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var errors = await _paymentService.ValidatePaymentAmountAsync(amount, currency);
            return Ok(new { valid = errors.Count == 0, errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment amount");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }
}
