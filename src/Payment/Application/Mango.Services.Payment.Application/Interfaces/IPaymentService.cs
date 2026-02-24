namespace Mango.Services.Payment.Application.Interfaces;

using Mango.Services.Payment.Application.DTOs;
using Mango.Services.Payment.Domain;

/// <summary>
/// Main payment service interface.
/// Orchestrates payment processing, refunds, and status checks.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Initiate a new payment (creates intent/order).
    /// </summary>
    /// <param name="request">Payment initiation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created payment with intent/order ID</returns>
    Task<PaymentDto> InitiatePaymentAsync(PaymentInitiateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm and process a payment.
    /// </summary>
    /// <param name="request">Payment confirmation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated payment with final status</returns>
    Task<PaymentDto> ConfirmPaymentAsync(PaymentConfirmRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refund a payment (fully or partially).
    /// </summary>
    /// <param name="request">Refund request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated payment with refund details</returns>
    Task<PaymentDto> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current payment status.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current payment status</returns>
    Task<PaymentStatusResponse> GetPaymentStatusAsync(int paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment history for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="skip">Number of records to skip</param>
    /// <param name="take">Number of records to take</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of payments</returns>
    Task<List<PaymentDto>> GetPaymentHistoryAsync(string userId, int skip = 0, int take = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payments for a specific order.
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of payments for order</returns>
    Task<List<PaymentDto>> GetOrderPaymentsAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a pending payment.
    /// </summary>
    /// <param name="paymentId">Payment ID to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated payment</returns>
    Task<PaymentDto> CancelPaymentAsync(int paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a webhook from payment gateway.
    /// </summary>
    /// <param name="gateway">Payment gateway that sent the webhook</param>
    /// <param name="webhookBody">Raw webhook body</param>
    /// <param name="signature">Webhook signature</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if webhook was processed successfully</returns>
    Task<bool> ProcessWebhookAsync(PaymentGateway gateway, string webhookBody, string signature, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available payment methods.
    /// </summary>
    /// <returns>List of supported payment methods</returns>
    Task<List<string>> GetAvailablePaymentMethodsAsync();

    /// <summary>
    /// Get supported currencies.
    /// </summary>
    /// <returns>List of supported currency codes</returns>
    Task<List<string>> GetSupportedCurrenciesAsync();

    /// <summary>
    /// Validate payment amount against configured limits.
    /// </summary>
    /// <param name="amount">Amount to validate</param>
    /// <param name="currency">Currency code</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    Task<List<string>> ValidatePaymentAmountAsync(decimal amount, string currency);
}
