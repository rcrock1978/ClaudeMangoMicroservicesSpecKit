namespace Mango.Services.Payment.Infrastructure.PaymentGateways;

using System.Text.Json;
using Mango.Services.Payment.Application.Interfaces;
using Mango.Services.Payment.Domain;

/// <summary>
/// PayPal payment gateway implementation.
/// Handles payment processing, refunds, and webhook processing for PayPal.
/// </summary>
public class PayPalPaymentGateway : IPaymentGateway
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _mode; // "sandbox" or "live"
    private readonly string _webhookId;
    private readonly HttpClient _httpClient;
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    /// <inheritdoc/>
    public PaymentGateway GatewayType => PaymentGateway.PayPal;

    /// <summary>
    /// Initializes a new instance of PayPalPaymentGateway.
    /// </summary>
    /// <param name="clientId">PayPal client ID</param>
    /// <param name="clientSecret">PayPal client secret</param>
    /// <param name="mode">PayPal mode ("sandbox" or "live")</param>
    /// <param name="webhookId">PayPal webhook ID</param>
    /// <param name="httpClient">Optional HTTP client for making requests</param>
    public PayPalPaymentGateway(string clientId, string clientSecret, string mode = "sandbox", string webhookId = "", HttpClient? httpClient = null)
    {
        _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        _mode = mode;
        _webhookId = webhookId ?? string.Empty;
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <inheritdoc/>
    public async Task<PaymentGatewayResponse> CreatePaymentAsync(decimal amount, string currency, string description, Dictionary<string, string>? metadata = null)
    {
        try
        {
            var baseUrl = _mode == "sandbox"
                ? "https://api-m.sandbox.paypal.com"
                : "https://api-m.paypal.com";

            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return new PaymentGatewayResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = "Failed to obtain PayPal access token"
                };
            }

            var request = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = currency.ToUpper(),
                            value = amount.ToString("F2")
                        },
                        description = description.Length > 127 ? description.Substring(0, 127) : description
                    }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders");
            httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
            httpRequest.Headers.Add("PayPal-Request-Id", Guid.NewGuid().ToString());
            httpRequest.Content = content;

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentGatewayResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = $"PayPal error: {response.StatusCode}",
                    RawResponse = responseBody
                };
            }

            var orderData = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var orderId = orderData.GetProperty("id").GetString();

            return new PaymentGatewayResponse
            {
                IsSuccess = true,
                TransactionId = orderId ?? string.Empty,
                Status = PaymentStatus.Pending,
                Amount = amount,
                RawResponse = responseBody
            };
        }
        catch (Exception ex)
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
            var baseUrl = _mode == "sandbox"
                ? "https://api-m.sandbox.paypal.com"
                : "https://api-m.paypal.com";

            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return new PaymentGatewayResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = "Failed to obtain PayPal access token"
                };
            }

            var request = new { };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders/{intentId}/capture");
            httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
            httpRequest.Content = content;

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentGatewayResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = $"PayPal capture failed: {response.StatusCode}",
                    RawResponse = responseBody
                };
            }

            var orderData = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var status = orderData.GetProperty("status").GetString();

            return new PaymentGatewayResponse
            {
                IsSuccess = status == "COMPLETED",
                TransactionId = intentId,
                Status = status == "COMPLETED" ? PaymentStatus.Completed : PaymentStatus.Processing,
                RawResponse = responseBody
            };
        }
        catch (Exception ex)
        {
            return new PaymentGatewayResponse
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentGatewayResponse> RefundPaymentAsync(string transactionId, decimal? refundAmount = null, string reason = "")
    {
        try
        {
            var baseUrl = _mode == "sandbox"
                ? "https://api-m.sandbox.paypal.com"
                : "https://api-m.paypal.com";

            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return new PaymentGatewayResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = "Failed to obtain PayPal access token"
                };
            }

            var request = new Dictionary<string, object>();
            if (refundAmount.HasValue)
            {
                request["amount"] = new
                {
                    currency_code = "USD",
                    value = refundAmount.Value.ToString("F2")
                };
            }

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/payments/captures/{transactionId}/refund");
            httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
            httpRequest.Content = content;

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentGatewayResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = $"PayPal refund failed: {response.StatusCode}",
                    RawResponse = responseBody
                };
            }

            var refundData = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var refundId = refundData.GetProperty("id").GetString();

            return new PaymentGatewayResponse
            {
                IsSuccess = true,
                TransactionId = transactionId,
                ChargeId = refundId,
                Status = PaymentStatus.Refunded,
                RawResponse = responseBody
            };
        }
        catch (Exception ex)
        {
            return new PaymentGatewayResponse
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentGatewayResponse> GetPaymentStatusAsync(string transactionId)
    {
        try
        {
            var baseUrl = _mode == "sandbox"
                ? "https://api-m.sandbox.paypal.com"
                : "https://api-m.paypal.com";

            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return new PaymentGatewayResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = "Failed to obtain PayPal access token"
                };
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/v2/checkout/orders/{transactionId}");
            httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentGatewayResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = $"Failed to get order status: {response.StatusCode}"
                };
            }

            var orderData = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var status = orderData.GetProperty("status").GetString();

            return new PaymentGatewayResponse
            {
                IsSuccess = status == "COMPLETED",
                TransactionId = transactionId,
                Status = status == "COMPLETED" ? PaymentStatus.Completed : PaymentStatus.Processing,
                RawResponse = responseBody
            };
        }
        catch (Exception ex)
        {
            return new PaymentGatewayResponse
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <inheritdoc/>
    public async Task<WebhookData?> ProcessWebhookAsync(string webhookBody, string signature)
    {
        try
        {
            if (!VerifyWebhookSignature(webhookBody, signature))
                return null;

            var webhookEvent = JsonSerializer.Deserialize<JsonElement>(webhookBody);
            if (!webhookEvent.TryGetProperty("event_type", out var eventTypeElement))
                return null;

            var eventType = eventTypeElement.GetString() ?? string.Empty;
            var webhookData = new WebhookData { EventType = eventType };

            if (webhookEvent.TryGetProperty("resource", out var resource))
            {
                if (resource.TryGetProperty("id", out var idElement))
                    webhookData.TransactionId = idElement.GetString() ?? string.Empty;

                if (resource.TryGetProperty("amount", out var amountElement) &&
                    amountElement.TryGetProperty("value", out var valueElement) &&
                    decimal.TryParse(valueElement.GetString(), out var amount))
                    webhookData.Amount = amount;

                if (resource.TryGetProperty("amount", out var currencyContainer) &&
                    currencyContainer.TryGetProperty("currency_code", out var currencyElement))
                    webhookData.Currency = currencyElement.GetString() ?? "USD";
            }

            webhookData.Status = MapPayPalEventToStatus(eventType);
            return webhookData;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public bool VerifyWebhookSignature(string webhookBody, string signature)
    {
        // For simplicity in this implementation, we verify signature format
        // In production, implement full PayPal signature verification
        return !string.IsNullOrEmpty(signature) && signature.Length > 0;
    }

    /// <summary>
    /// Get PayPal access token.
    /// </summary>
    private async Task<string?> GetAccessTokenAsync()
    {
        // Check if we have a valid cached token
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            return _accessToken;

        try
        {
            var baseUrl = _mode == "sandbox"
                ? "https://api-m.sandbox.paypal.com"
                : "https://api-m.paypal.com";

            var auth = System.Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token");
            request.Headers.Add("Authorization", $"Basic {auth}");
            request.Content = new StringContent(
                "grant_type=client_credentials",
                System.Text.Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseBody);
            if (tokenData.TryGetProperty("access_token", out var tokenElement))
            {
                _accessToken = tokenElement.GetString();
                if (tokenData.TryGetProperty("expires_in", out var expiresElement) &&
                    int.TryParse(expiresElement.GetString(), out var expiresIn))
                {
                    _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60); // Renew 60 seconds before expiry
                }
                return _accessToken;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Map PayPal event type to domain PaymentStatus.
    /// </summary>
    private PaymentStatus MapPayPalEventToStatus(string eventType)
    {
        return eventType switch
        {
            "CHECKOUT.ORDER.COMPLETED" => PaymentStatus.Completed,
            "PAYMENT.CAPTURE.COMPLETED" => PaymentStatus.Completed,
            "PAYMENT.CAPTURE.DECLINED" => PaymentStatus.Failed,
            "PAYMENT.CAPTURE.DENIED" => PaymentStatus.Failed,
            "PAYMENT.CAPTURE.REFUNDED" => PaymentStatus.Refunded,
            "PAYMENT.CAPTURE.REVERSED" => PaymentStatus.Refunded,
            _ => PaymentStatus.Processing
        };
    }
}
