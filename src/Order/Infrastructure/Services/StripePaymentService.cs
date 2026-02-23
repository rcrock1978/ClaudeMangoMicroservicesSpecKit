using Mango.Services.Order.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Mango.Services.Order.Infrastructure.Services;

/// <summary>
/// Stripe payment service implementation.
/// Handles checkout session creation and payment validation via Stripe API.
/// </summary>
public class StripePaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Initialize Stripe API key from configuration
        var stripeSecretKey = configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(stripeSecretKey))
        {
            throw new InvalidOperationException("Stripe:SecretKey not configured");
        }

        StripeConfiguration.ApiKey = stripeSecretKey;
    }

    public async Task<CreateCheckoutSessionResult> CreateCheckoutSessionAsync(
        OrderCheckoutDetails order,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Stripe checkout session for order {OrderId}", order.OrderId);

            var lineItems = order.Items.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Price * 100), // Convert to cents
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.ProductName,
                        Images = new List<string> { /* product image URLs */ }
                    }
                },
                Quantity = item.Quantity
            }).ToList();

            var options = new SessionCreateOptions
            {
                SuccessUrl = order.SuccessUrl ?? "http://localhost:7200/checkout-success",
                CancelUrl = order.CancelUrl ?? "http://localhost:7200/checkout-cancel",
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                CustomerEmail = order.Email,
                Metadata = new Dictionary<string, string>
                {
                    { "orderId", order.OrderId.ToString() },
                    { "userId", order.UserId }
                }
            };

            var session = await Session.CreateAsync(options, null, cancellationToken);

            _logger.LogInformation("Stripe session created: {SessionId} for order {OrderId}", session.Id, order.OrderId);

            return new CreateCheckoutSessionResult
            {
                IsSuccess = true,
                SessionId = session.Id,
                CheckoutUrl = session.Url
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating checkout session for order {OrderId}: {Message}", order.OrderId, ex.Message);
            return new CreateCheckoutSessionResult
            {
                IsSuccess = false,
                ErrorMessage = $"Failed to create payment session: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating checkout session for order {OrderId}", order.OrderId);
            return new CreateCheckoutSessionResult
            {
                IsSuccess = false,
                ErrorMessage = "An unexpected error occurred"
            };
        }
    }

    public async Task<ValidatePaymentResult> ValidatePaymentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating Stripe payment intent: {PaymentIntentId}", paymentIntentId);

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId, null, cancellationToken);

            var isConfirmed = paymentIntent.Status == "succeeded";

            _logger.LogInformation(
                "Stripe payment intent {PaymentIntentId} status: {Status}, Confirmed: {IsConfirmed}",
                paymentIntentId, paymentIntent.Status, isConfirmed);

            return new ValidatePaymentResult
            {
                IsSuccess = true,
                IsPaymentConfirmed = isConfirmed,
                PaymentIntentId = paymentIntent.Id,
                Amount = (decimal)(paymentIntent.Amount ?? 0) / 100 // Convert from cents
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error validating payment intent {PaymentIntentId}: {Message}", paymentIntentId, ex.Message);
            return new ValidatePaymentResult
            {
                IsSuccess = false,
                ErrorMessage = $"Failed to validate payment: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating payment intent {PaymentIntentId}", paymentIntentId);
            return new ValidatePaymentResult
            {
                IsSuccess = false,
                ErrorMessage = "An unexpected error occurred"
            };
        }
    }

    public async Task<RefundResult> RefundAsync(
        string paymentIntentId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating refund for payment intent {PaymentIntentId}, amount: {Amount}", paymentIntentId, amount);

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = (long)(amount * 100) // Convert to cents
            }, null, cancellationToken);

            _logger.LogInformation("Refund created: {RefundId} for payment intent {PaymentIntentId}", refund.Id, paymentIntentId);

            return new RefundResult
            {
                IsSuccess = refund.Status == "succeeded",
                RefundId = refund.Id
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error refunding payment intent {PaymentIntentId}: {Message}", paymentIntentId, ex.Message);
            return new RefundResult
            {
                IsSuccess = false,
                ErrorMessage = $"Failed to refund payment: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error refunding payment intent {PaymentIntentId}", paymentIntentId);
            return new RefundResult
            {
                IsSuccess = false,
                ErrorMessage = "An unexpected error occurred"
            };
        }
    }
}
