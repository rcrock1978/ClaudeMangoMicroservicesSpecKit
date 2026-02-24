namespace Mango.Services.Payment.Application.Services;

using Mango.Services.Payment.Application.DTOs;
using Mango.Services.Payment.Application.Interfaces;
using Mango.Services.Payment.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Main payment service implementation.
/// Orchestrates payment processing, refunds, and status checks.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;
    private readonly decimal _minimumAmount;
    private readonly decimal _maximumAmount;
    private readonly List<string> _supportedCurrencies;

    /// <summary>
    /// Initializes a new instance of PaymentService.
    /// </summary>
    public PaymentService(
        IPaymentRepository repository,
        IPaymentGatewayFactory gatewayFactory,
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _gatewayFactory = gatewayFactory ?? throw new ArgumentNullException(nameof(gatewayFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Load configuration
        _minimumAmount = decimal.Parse(_configuration["PaymentGateway:MinimumAmount"] ?? "0.50");
        _maximumAmount = decimal.Parse(_configuration["PaymentGateway:MaximumAmount"] ?? "99999.99");
        _supportedCurrencies = _configuration
            .GetSection("PaymentGateway:CurrenciesSupported")
            .Get<List<string>>() ?? new List<string> { "USD", "EUR", "GBP" };
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> InitiatePaymentAsync(PaymentInitiateRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request
        var validationErrors = ValidateInitiateRequest(request);
        if (validationErrors.Any())
        {
            _logger.LogWarning("Payment initiation validation failed: {Errors}", string.Join(", ", validationErrors));
            throw new ArgumentException($"Invalid payment request: {string.Join(", ", validationErrors)}");
        }

        try
        {
            // Create payment entity
            var payment = new Payment
            {
                OrderId = request.OrderId,
                UserId = request.OrderId.ToString(), // Should come from context
                Amount = request.Amount,
                Currency = request.Currency,
                Method = request.Method,
                Gateway = request.Gateway ?? _gatewayFactory.GetDefaultGateway().GatewayType,
                Status = PaymentStatus.Pending,
                PayerIpAddress = request.PayerIpAddress,
                CardHolderName = request.CardHolderName,
                CardExpiryMonth = request.CardExpiryMonth,
                CardExpiryYear = request.CardExpiryYear,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save to database
            payment = await _repository.CreatePaymentAsync(payment);

            // Create payment intent/order with gateway
            var gateway = _gatewayFactory.CreateGateway(payment.Gateway);
            var gatewayResponse = await gateway.CreatePaymentAsync(
                payment.Amount,
                payment.Currency,
                $"Order #{payment.OrderId}",
                new Dictionary<string, string>
                {
                    { "order_id", payment.OrderId.ToString() },
                    { "user_id", payment.UserId }
                });

            if (!gatewayResponse.IsSuccess)
            {
                payment.Status = PaymentStatus.Failed;
                payment.ErrorMessage = gatewayResponse.ErrorMessage ?? "Failed to create payment intent";
                await _repository.UpdatePaymentAsync(payment);

                _logger.LogError("Payment intent creation failed: {Error}", gatewayResponse.ErrorMessage);
                throw new InvalidOperationException($"Failed to create payment: {gatewayResponse.ErrorMessage}");
            }

            payment.TransactionId = gatewayResponse.TransactionId;
            payment.GatewayResponseData = gatewayResponse.RawResponse;
            await _repository.UpdatePaymentAsync(payment);

            // Log the event
            await LogPaymentEventAsync(payment, "PAYMENT_INITIATED", PaymentStatus.Pending, "Payment intent created");

            return MapToDto(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for order {OrderId}", request.OrderId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> ConfirmPaymentAsync(PaymentConfirmRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await _repository.GetByIdAsync(request.PaymentId)
            ?? throw new InvalidOperationException($"Payment {request.PaymentId} not found");

        if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Processing)
        {
            throw new InvalidOperationException($"Payment cannot be confirmed in {payment.Status} status");
        }

        try
        {
            payment.Status = PaymentStatus.Processing;
            await _repository.UpdatePaymentAsync(payment);

            // Confirm payment with gateway
            var gateway = _gatewayFactory.CreateGateway(payment.Gateway);
            var intentId = request.StripePaymentIntentId ?? request.PayPalOrderId ?? payment.TransactionId;

            var gatewayResponse = await gateway.ConfirmPaymentAsync(intentId, request.ConfirmationToken);

            if (!gatewayResponse.IsSuccess)
            {
                payment.Status = PaymentStatus.Failed;
                payment.ErrorMessage = gatewayResponse.ErrorMessage ?? "Payment confirmation failed";
                await _repository.UpdatePaymentAsync(payment);

                _logger.LogError("Payment confirmation failed: {Error}", gatewayResponse.ErrorMessage);
                await LogPaymentEventAsync(payment, "PAYMENT_FAILED", PaymentStatus.Failed, gatewayResponse.ErrorMessage ?? "Confirmation failed");
                throw new InvalidOperationException($"Payment failed: {gatewayResponse.ErrorMessage}");
            }

            // Update payment with successful response
            payment.TransactionId = gatewayResponse.TransactionId;
            payment.Status = PaymentStatus.Completed;
            payment.PaymentDate = DateTime.UtcNow;
            payment.CardLast4 = gatewayResponse.CardLast4;
            payment.GatewayResponseData = gatewayResponse.RawResponse;
            payment.ErrorMessage = null;

            await _repository.UpdatePaymentAsync(payment);
            await LogPaymentEventAsync(payment, "PAYMENT_COMPLETED", PaymentStatus.Completed, "Payment successfully processed");

            _logger.LogInformation("Payment {PaymentId} confirmed for order {OrderId}", payment.Id, payment.OrderId);
            return MapToDto(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment {PaymentId}", request.PaymentId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await _repository.GetByIdAsync(request.PaymentId)
            ?? throw new InvalidOperationException($"Payment {request.PaymentId} not found");

        if (!payment.CanBeRefunded())
        {
            throw new InvalidOperationException($"Payment cannot be refunded in {payment.Status} status or is already fully refunded");
        }

        try
        {
            var refundAmount = request.RefundAmount ?? payment.Amount;

            if (refundAmount <= 0 || refundAmount > payment.GetRefundableAmount())
            {
                throw new ArgumentException($"Invalid refund amount: {refundAmount}");
            }

            // Create refund record
            var refund = new PaymentRefund
            {
                PaymentId = payment.Id,
                RefundAmount = refundAmount,
                Reason = request.Reason,
                Status = PaymentStatus.Processing,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.RecordRefundAsync(refund);

            // Process refund with gateway
            var gateway = _gatewayFactory.CreateGateway(payment.Gateway);
            var gatewayResponse = await gateway.RefundPaymentAsync(payment.TransactionId, refundAmount, request.Reason);

            if (!gatewayResponse.IsSuccess)
            {
                refund.Status = PaymentStatus.Failed;
                refund.ErrorMessage = gatewayResponse.ErrorMessage;
                await _repository.RecordRefundAsync(refund);

                _logger.LogError("Refund failed for payment {PaymentId}: {Error}", payment.Id, gatewayResponse.ErrorMessage);
                throw new InvalidOperationException($"Refund failed: {gatewayResponse.ErrorMessage}");
            }

            // Update payment record
            refund.Status = PaymentStatus.Completed;
            refund.GatewayRefundId = gatewayResponse.ChargeId;
            refund.ProcessedDate = DateTime.UtcNow;
            await _repository.RecordRefundAsync(refund);

            payment.RefundedAmount += refundAmount;
            if (payment.RefundedAmount >= payment.Amount)
            {
                payment.Status = PaymentStatus.Refunded;
            }
            payment.RefundDate = DateTime.UtcNow;
            await _repository.UpdatePaymentAsync(payment);

            await LogPaymentEventAsync(payment, "PAYMENT_REFUNDED", payment.Status, $"Refund of {refundAmount} {payment.Currency} processed");

            _logger.LogInformation("Refund of {Amount} processed for payment {PaymentId}", refundAmount, payment.Id);
            return MapToDto(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", request.PaymentId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentStatusResponse> GetPaymentStatusAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _repository.GetByIdAsync(paymentId)
            ?? throw new InvalidOperationException($"Payment {paymentId} not found");

        return new PaymentStatusResponse
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            Status = payment.Status,
            Amount = payment.Amount,
            Currency = payment.Currency,
            RefundedAmount = payment.RefundedAmount,
            RefundableAmount = payment.GetRefundableAmount(),
            TransactionId = payment.TransactionId,
            CardLast4 = payment.CardLast4,
            PaymentDate = payment.PaymentDate,
            RefundDate = payment.RefundDate,
            ErrorMessage = payment.ErrorMessage,
            Gateway = payment.Gateway
        };
    }

    /// <inheritdoc/>
    public async Task<List<PaymentDto>> GetPaymentHistoryAsync(string userId, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new List<PaymentDto>();

        var payments = await _repository.GetByUserIdAsync(userId);
        return payments
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(MapToDto)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<PaymentDto>> GetOrderPaymentsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var payments = await _repository.GetByOrderIdAsync(orderId);
        return payments.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> CancelPaymentAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _repository.GetByIdAsync(paymentId)
            ?? throw new InvalidOperationException($"Payment {paymentId} not found");

        if (!payment.CancelPayment())
        {
            throw new InvalidOperationException($"Payment cannot be cancelled from {payment.Status} status");
        }

        await _repository.UpdatePaymentAsync(payment);
        await LogPaymentEventAsync(payment, "PAYMENT_CANCELLED", PaymentStatus.Cancelled, "Payment was cancelled");

        return MapToDto(payment);
    }

    /// <inheritdoc/>
    public async Task<bool> ProcessWebhookAsync(PaymentGateway gateway, string webhookBody, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var gatewayImpl = _gatewayFactory.CreateGateway(gateway);
            var webhookData = await gatewayImpl.ProcessWebhookAsync(webhookBody, signature);

            if (webhookData == null)
            {
                _logger.LogWarning("Invalid webhook signature from {Gateway}", gateway);
                return false;
            }

            var payment = await _repository.GetByTransactionIdAsync(webhookData.TransactionId);
            if (payment == null)
            {
                _logger.LogWarning("Webhook received for unknown transaction: {TransactionId}", webhookData.TransactionId);
                return false;
            }

            // Update payment status based on webhook event
            payment.Status = webhookData.Status;
            if (webhookData.Status == PaymentStatus.Completed && payment.PaymentDate == null)
            {
                payment.PaymentDate = webhookData.EventTime;
            }

            await _repository.UpdatePaymentAsync(payment);
            await LogPaymentEventAsync(payment, webhookData.EventType, webhookData.Status, $"Webhook: {webhookData.EventType}");

            _logger.LogInformation("Webhook processed for payment {PaymentId}: {EventType}", payment.Id, webhookData.EventType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook from {Gateway}", gateway);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetAvailablePaymentMethodsAsync()
    {
        return Enum.GetNames(typeof(PaymentMethod))
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetSupportedCurrenciesAsync()
    {
        return await Task.FromResult(_supportedCurrencies);
    }

    /// <inheritdoc/>
    public async Task<List<string>> ValidatePaymentAmountAsync(decimal amount, string currency)
    {
        var errors = new List<string>();

        if (amount < _minimumAmount)
            errors.Add($"Amount must be at least {_minimumAmount:C}");

        if (amount > _maximumAmount)
            errors.Add($"Amount cannot exceed {_maximumAmount:C}");

        if (!_supportedCurrencies.Contains(currency.ToUpper()))
            errors.Add($"Currency {currency} is not supported");

        return await Task.FromResult(errors);
    }

    /// <summary>
    /// Validate payment initiation request.
    /// </summary>
    private List<string> ValidateInitiateRequest(PaymentInitiateRequest request)
    {
        var errors = new List<string>();

        if (request.OrderId <= 0)
            errors.Add("OrderId must be greater than 0");

        if (request.Amount <= 0)
            errors.Add("Amount must be greater than 0");

        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
            errors.Add("Currency must be a valid 3-letter code");

        if (!_supportedCurrencies.Contains(request.Currency.ToUpper()))
            errors.Add($"Currency {request.Currency} is not supported");

        if (request.Amount < _minimumAmount)
            errors.Add($"Amount must be at least {_minimumAmount}");

        if (request.Amount > _maximumAmount)
            errors.Add($"Amount cannot exceed {_maximumAmount}");

        return errors;
    }

    /// <summary>
    /// Map Payment entity to PaymentDto.
    /// </summary>
    private PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            Method = payment.Method,
            Gateway = payment.Gateway,
            TransactionId = payment.TransactionId,
            CardLast4 = payment.CardLast4,
            PaymentDate = payment.PaymentDate,
            RefundedAmount = payment.RefundedAmount,
            RefundDate = payment.RefundDate,
            ErrorMessage = payment.ErrorMessage,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }

    /// <summary>
    /// Log a payment event for audit trail.
    /// </summary>
    private async Task LogPaymentEventAsync(Payment payment, string eventType, PaymentStatus status, string message)
    {
        var log = new PaymentLog
        {
            PaymentId = payment.Id,
            TransactionId = payment.TransactionId,
            EventType = eventType,
            Status = status,
            Message = message,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddPaymentLogAsync(log);
    }
}
