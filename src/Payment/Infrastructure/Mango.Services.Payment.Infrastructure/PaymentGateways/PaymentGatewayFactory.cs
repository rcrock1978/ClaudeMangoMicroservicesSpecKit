namespace Mango.Services.Payment.Infrastructure.PaymentGateways;

using Microsoft.Extensions.Configuration;
using Mango.Services.Payment.Application.Interfaces;
using Mango.Services.Payment.Domain;

/// <summary>
/// Factory for creating payment gateway instances.
/// Allows selecting between Stripe and PayPal based on configuration.
/// </summary>
public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of PaymentGatewayFactory.
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <param name="httpClient">HTTP client for API calls</param>
    public PaymentGatewayFactory(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc/>
    public IPaymentGateway CreateGateway(PaymentGateway gateway)
    {
        return gateway switch
        {
            PaymentGateway.Stripe => CreateStripeGateway(),
            PaymentGateway.PayPal => CreatePayPalGateway(),
            _ => throw new ArgumentException($"Unsupported payment gateway: {gateway}")
        };
    }

    /// <inheritdoc/>
    public IPaymentGateway GetDefaultGateway()
    {
        var defaultGateway = _configuration["PaymentGateway:Default"] ?? "Stripe";
        var gateway = Enum.Parse<PaymentGateway>(defaultGateway);
        return CreateGateway(gateway);
    }

    /// <summary>
    /// Create Stripe payment gateway instance.
    /// </summary>
    private IPaymentGateway CreateStripeGateway()
    {
        var secretKey = _configuration["PaymentGateway:Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe SecretKey configuration is missing");

        var webhookSecret = _configuration["PaymentGateway:Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe WebhookSecret configuration is missing");

        return new StripePaymentGateway(secretKey, webhookSecret);
    }

    /// <summary>
    /// Create PayPal payment gateway instance.
    /// </summary>
    private IPaymentGateway CreatePayPalGateway()
    {
        var clientId = _configuration["PaymentGateway:PayPal:ClientId"]
            ?? throw new InvalidOperationException("PayPal ClientId configuration is missing");

        var clientSecret = _configuration["PaymentGateway:PayPal:Secret"]
            ?? throw new InvalidOperationException("PayPal Secret configuration is missing");

        var mode = _configuration["PaymentGateway:PayPal:Mode"] ?? "sandbox";
        var webhookId = _configuration["PaymentGateway:PayPal:WebhookId"] ?? string.Empty;

        return new PayPalPaymentGateway(clientId, clientSecret, mode, webhookId, _httpClient);
    }
}
