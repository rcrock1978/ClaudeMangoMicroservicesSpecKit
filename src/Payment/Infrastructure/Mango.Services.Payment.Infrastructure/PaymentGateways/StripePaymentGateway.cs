namespace Mango.Services.Payment.Infrastructure.PaymentGateways;

using System.Text.Json;
using Mango.Services.Payment.Application.Interfaces;
using Mango.Services.Payment.Domain;
using Stripe;
using Stripe.Checkout;

/// <summary>
/// Stripe payment gateway implementation.
/// Handles payment processing, refunds, and webhook processing for Stripe.
/// </summary>
public class StripePaymentGateway : IPaymentGateway
{
    private readonly string _secretKey;
    private readonly string _webhookSecret;

    /// <inheritdoc/>
    public PaymentGateway GatewayType => PaymentGateway.Stripe;

    /// <summary>
    /// Initializes a new instance of StripePaymentGateway.
    /// </summary>
    /// <param name="secretKey">Stripe secret API key</param>
    /// <param name="webhookSecret">Stripe webhook secret for signature verification</param>
    public StripePaymentGateway(string secretKey, string webhookSecret)
    {
        _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        _webhookSecret = webhookSecret ?? throw new ArgumentNullException(nameof(webhookSecret));
        StripeConfiguration.ApiKey = _secretKey;
    }

    /// <inheritdoc/>
    public async Task<PaymentGatewayResponse> CreatePaymentAsync(decimal amount, string currency, string description, Dictionary<string, string>? metadata = null)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = currency.ToLower(),
                Description = description,
                StatementDescriptor = description.Length > 22 ? description.Substring(0, 22) : description,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return new PaymentGatewayResponse
            {
                IsSuccess = true,
                TransactionId = paymentIntent.Id,
                Status = MapStripeStatus(paymentIntent.Status),
                Amount = amount,
                RawResponse = JsonSerializer.Serialize(paymentIntent)
            };
        }
        catch (StripeException ex)
        {
            return new PaymentGatewayResponse
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorMessage = ex.Message,
                Amount = amount
            };
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentGatewayResponse> ConfirmPaymentAsync(string intentId, string? cardToken = null, Dictionary<string, string>? metadata = null)
    {
        try
        {
            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = cardToken
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.ConfirmAsync(intentId, options);

            var response = new PaymentGatewayResponse
            {
                IsSuccess = paymentIntent.Status == "succeeded",
                TransactionId = paymentIntent.Id,
                ChargeId = paymentIntent.LatestChargeId,
                Status = MapStripeStatus(paymentIntent.Status),
                Amount = (decimal)paymentIntent.Amount / 100,
                RawResponse = JsonSerializer.Serialize(paymentIntent)
            };

            // Retrieve charge details for card information
            if (!string.IsNullOrEmpty(paymentIntent.LatestChargeId))
            {
                var chargeService = new ChargeService();
                var charge = await chargeService.GetAsync(paymentIntent.LatestChargeId);

                if (charge?.PaymentMethodDetails?.Card != null)
                {
                    response.CardLast4 = charge.PaymentMethodDetails.Card.Last4;
                    response.CardBrand = charge.PaymentMethodDetails.Card.Brand;
                }
            }

            if (!response.IsSuccess)
            {
                response.ErrorMessage = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
            }

            return response;
        }
        catch (StripeException ex)
        {
            return new PaymentGatewayResponse
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorMessage = ex.Message,
                TransactionId = intentId
            };
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentGatewayResponse> RefundPaymentAsync(string transactionId, decimal? refundAmount = null, string reason = "")
    {
        try
        {
            var options = new RefundCreateOptions
            {
                Charge = transactionId,
                Reason = string.IsNullOrEmpty(reason) ? RefundReasons.RequestedByCustomer : reason,
                Metadata = new Dictionary<string, string> { { "reason", reason } }
            };

            if (refundAmount.HasValue)
            {
                options.Amount = (long)(refundAmount.Value * 100); // Convert to cents
            }

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            return new PaymentGatewayResponse
            {
                IsSuccess = refund.Status == "succeeded",
                TransactionId = refund.ChargeId!,
                ChargeId = refund.Id,
                Status = refund.Status == "succeeded" ? PaymentStatus.Refunded : PaymentStatus.Failed,
                Amount = refund.Amount > 0 ? (decimal)refund.Amount / 100 : 0,
                RawResponse = JsonSerializer.Serialize(refund),
                ErrorMessage = refund.Status == "failed" ? "Refund failed" : null
            };
        }
        catch (StripeException ex)
        {
            return new PaymentGatewayResponse
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorMessage = ex.Message,
                TransactionId = transactionId
            };
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentGatewayResponse> GetPaymentStatusAsync(string transactionId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(transactionId);

            return new PaymentGatewayResponse
            {
                IsSuccess = paymentIntent.Status == "succeeded",
                TransactionId = paymentIntent.Id,
                ChargeId = paymentIntent.LatestChargeId,
                Status = MapStripeStatus(paymentIntent.Status),
                Amount = (decimal)paymentIntent.Amount / 100,
                RawResponse = JsonSerializer.Serialize(paymentIntent)
            };
        }
        catch (StripeException ex)
        {
            return new PaymentGatewayResponse
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorMessage = ex.Message,
                TransactionId = transactionId
            };
        }
    }

    /// <inheritdoc/>
    public async Task<WebhookData?> ProcessWebhookAsync(string webhookBody, string signature)
    {
        try
        {
            // Verify webhook signature
            if (!VerifyWebhookSignature(webhookBody, signature))
                return null;

            var stripeEvent = EventUtility.ParseEvent(webhookBody);
            if (stripeEvent?.Data?.Object == null)
                return null;

            return MapStripeEventToWebhookData(stripeEvent);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public bool VerifyWebhookSignature(string webhookBody, string signature)
    {
        try
        {
            EventUtility.ValidateSignature(webhookBody, signature, _webhookSecret);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Map Stripe payment intent status to domain PaymentStatus.
    /// </summary>
    private PaymentStatus MapStripeStatus(string stripeStatus)
    {
        return stripeStatus switch
        {
            "requires_payment_method" => PaymentStatus.Pending,
            "requires_confirmation" => PaymentStatus.Pending,
            "processing" => PaymentStatus.Processing,
            "succeeded" => PaymentStatus.Completed,
            "requires_action" => PaymentStatus.Processing,
            "canceled" => PaymentStatus.Cancelled,
            _ => PaymentStatus.Failed
        };
    }

    /// <summary>
    /// Map Stripe event to WebhookData.
    /// </summary>
    private WebhookData MapStripeEventToWebhookData(Event stripeEvent)
    {
        var webhookData = new WebhookData
        {
            EventType = stripeEvent.Type,
            EventTime = UnixTimeStampToDateTime(stripeEvent.Created)
        };

        if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
        {
            webhookData.TransactionId = paymentIntent.Id;
            webhookData.Amount = (decimal)paymentIntent.Amount / 100;
            webhookData.Currency = paymentIntent.Currency;
            webhookData.Status = MapStripeStatus(paymentIntent.Status);
            webhookData.Data = new Dictionary<string, object>
            {
                { "charge_id", paymentIntent.LatestChargeId ?? "" }
            };
        }
        else if (stripeEvent.Data.Object is Charge charge)
        {
            webhookData.TransactionId = charge.PaymentIntentId ?? charge.Id;
            webhookData.Amount = (decimal)charge.Amount / 100;
            webhookData.Currency = charge.Currency;
            webhookData.Status = charge.Status == "succeeded" ? PaymentStatus.Completed : PaymentStatus.Failed;
            webhookData.Data = new Dictionary<string, object>
            {
                { "charge_id", charge.Id }
            };
        }

        return webhookData;
    }

    /// <summary>
    /// Convert Unix timestamp to DateTime.
    /// </summary>
    private static DateTime UnixTimeStampToDateTime(DateTime? dateTime)
    {
        return dateTime ?? DateTime.UtcNow;
    }
}
