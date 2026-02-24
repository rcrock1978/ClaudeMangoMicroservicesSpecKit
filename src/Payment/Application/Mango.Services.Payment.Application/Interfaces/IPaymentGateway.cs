namespace Mango.Services.Payment.Application.Interfaces;

using Mango.Services.Payment.Domain;

/// <summary>
/// Interface for payment gateway abstraction (Stripe, PayPal, etc).
/// Provides common operations for different payment providers.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Get the gateway type this implementation handles.
    /// </summary>
    PaymentGateway GatewayType { get; }

    /// <summary>
    /// Create a payment intent or order (first step of payment).
    /// </summary>
    /// <param name="amount">Amount to charge</param>
    /// <param name="currency">Currency code</param>
    /// <param name="description">Payment description</param>
    /// <param name="metadata">Additional metadata</param>
    /// <returns>Payment intent/order ID or token</returns>
    Task<PaymentGatewayResponse> CreatePaymentAsync(decimal amount, string currency, string description, Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Confirm and charge a payment.
    /// </summary>
    /// <param name="intentId">Payment intent/order ID from CreatePayment</param>
    /// <param name="cardToken">Card token (if using card)</param>
    /// <param name="metadata">Additional metadata</param>
    /// <returns>Transaction details on success</returns>
    Task<PaymentGatewayResponse> ConfirmPaymentAsync(string intentId, string? cardToken = null, Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Refund a payment (fully or partially).
    /// </summary>
    /// <param name="transactionId">Original transaction/charge ID</param>
    /// <param name="refundAmount">Amount to refund (null for full refund)</param>
    /// <param name="reason">Refund reason</param>
    /// <returns>Refund details</returns>
    Task<PaymentGatewayResponse> RefundPaymentAsync(string transactionId, decimal? refundAmount = null, string reason = "");

    /// <summary>
    /// Get current status of a payment.
    /// </summary>
    /// <param name="transactionId">Transaction ID to check</param>
    /// <returns>Current status and details</returns>
    Task<PaymentGatewayResponse> GetPaymentStatusAsync(string transactionId);

    /// <summary>
    /// Process webhook from payment gateway.
    /// </summary>
    /// <param name="webhookBody">Raw webhook body</param>
    /// <param name="signature">Webhook signature for verification</param>
    /// <returns>Webhook data if valid</returns>
    Task<WebhookData?> ProcessWebhookAsync(string webhookBody, string signature);

    /// <summary>
    /// Verify webhook signature.
    /// </summary>
    /// <param name="webhookBody">Raw webhook body</param>
    /// <param name="signature">Signature to verify</param>
    /// <returns>True if signature is valid</returns>
    bool VerifyWebhookSignature(string webhookBody, string signature);
}

/// <summary>
/// Standard response from payment gateway operations.
/// </summary>
public class PaymentGatewayResponse
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Transaction ID or order ID from the gateway.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Payment status after operation.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Charge/Refund ID from gateway.
    /// </summary>
    public string? ChargeId { get; set; }

    /// <summary>
    /// Amount processed.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Error message if operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Raw response data from gateway (JSON).
    /// </summary>
    public string? RawResponse { get; set; }

    /// <summary>
    /// Last 4 digits of card (if applicable).
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// Card brand (Visa, Mastercard, etc).
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Authorization code from gateway.
    /// </summary>
    public string? AuthorizationCode { get; set; }

    /// <summary>
    /// Risk level assessment (Low, Medium, High).
    /// </summary>
    public string? RiskLevel { get; set; }
}

/// <summary>
/// Data parsed from a webhook event.
/// </summary>
public class WebhookData
{
    /// <summary>
    /// Event type (payment.succeeded, charge.refunded, etc).
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Transaction ID from the event.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Amount involved in the event.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency of the transaction.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payment status from the event.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Additional data from the webhook.
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// When the event occurred.
    /// </summary>
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Interface for payment gateway factory.
/// </summary>
public interface IPaymentGatewayFactory
{
    /// <summary>
    /// Create a payment gateway instance.
    /// </summary>
    /// <param name="gateway">Gateway type to create</param>
    /// <returns>Payment gateway instance</returns>
    IPaymentGateway CreateGateway(PaymentGateway gateway);

    /// <summary>
    /// Get the default payment gateway configured in settings.
    /// </summary>
    /// <returns>Default payment gateway instance</returns>
    IPaymentGateway GetDefaultGateway();
}
