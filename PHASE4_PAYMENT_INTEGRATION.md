# Phase 4: Payment Integration - Complete Implementation Guide

## Overview

Phase 4 implements a comprehensive payment service with support for both Stripe and PayPal payment gateways. The implementation follows clean architecture principles with domain-driven design, featuring full PCI DSS compliance, event integration, and production-ready security.

## Architecture

### Project Structure

```
src/Payment/
├── Domain/Mango.Services.Payment.Domain/
│   ├── BaseEntity.cs
│   ├── PaymentStatus.cs (enum)
│   ├── PaymentMethod.cs (enum)
│   ├── PaymentGateway.cs (enum)
│   ├── Payment.cs (aggregate root)
│   ├── PaymentRefund.cs
│   └── PaymentLog.cs
├── Application/Mango.Services.Payment.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── MediatR/
│   ├── Services/
│   └── Mango.Services.Payment.Application.csproj
├── Infrastructure/Mango.Services.Payment.Infrastructure/
│   ├── Data/PaymentDbContext.cs
│   ├── Repositories/PaymentRepository.cs
│   ├── PaymentGateways/
│   │   ├── StripePaymentGateway.cs
│   │   ├── PayPalPaymentGateway.cs
│   │   └── PaymentGatewayFactory.cs
│   └── Mango.Services.Payment.Infrastructure.csproj
└── API/Mango.Services.Payment.API/
    ├── Controllers/PaymentController.cs
    ├── Program.cs
    ├── appsettings.json
    └── Mango.Services.Payment.API.csproj

tests/Mango.Services.Payment.UnitTests/
├── Domain/
│   ├── PaymentTests.cs (14+ tests)
│   └── PaymentRefundTests.cs (8+ tests)
└── Mango.Services.Payment.UnitTests.csproj
```

## Domain Entities

### Payment Aggregate Root

Main payment entity with state machine pattern:

- **Properties:**
  - OrderId: Associated order
  - UserId: Customer ID
  - Amount: Payment amount (decimal)
  - Currency: 3-letter currency code
  - Status: Current payment status
  - Method: Payment method used
  - Gateway: Stripe or PayPal
  - TransactionId: Gateway transaction ID
  - CardDetails: Encrypted card information
  - PaymentDate: When payment completed
  - RefundedAmount: Total refunded
  - RefundDate: When refund processed

- **Methods:**
  - InitiatePayment(): Start payment process
  - CompletePayment(): Mark as completed
  - FailPayment(): Record failure
  - RefundPayment(): Process refund
  - CancelPayment(): Cancel pending payment
  - CanBeRefunded(): Check refund eligibility
  - GetRefundableAmount(): Remaining refundable amount
  - IsTerminal(): Check if in final state

### PaymentRefund Entity

Tracks refund transactions:

- RefundAmount: Amount being refunded
- Reason: Refund reason
- Status: Refund status
- ProcessedDate: When processed
- GatewayRefundId: Refund ID from gateway
- ErrorMessage: Failure reason if applicable

### PaymentLog Entity

Audit trail for compliance:

- TransactionId: Gateway transaction ID
- EventType: Type of event
- Status: Payment status
- Message: Event description
- ErrorMessage: Error if applicable
- ResponseData: Gateway response (JSON)
- Timestamp: When event occurred

## API Endpoints

### Payment Operations

```
POST   /api/payment/initiate           - Create payment intent/order
POST   /api/payment/confirm            - Confirm and process payment
POST   /api/payment/refund             - Refund payment (full or partial)
GET    /api/payment/{paymentId}        - Get payment status
GET    /api/payment/user/history       - Get user's payment history
GET    /api/payment/order/{orderId}    - Get order payments
GET    /api/payment/methods            - List available payment methods
GET    /api/payment/currencies         - List supported currencies
POST   /api/payment/{paymentId}/cancel - Cancel pending payment
GET    /api/payment/validate           - Validate amount and currency
POST   /api/payment/webhook/{gateway}  - Handle gateway webhooks
```

### Request/Response Examples

#### Initiate Payment
```json
POST /api/payment/initiate
{
  "orderId": 123,
  "amount": 99.99,
  "currency": "USD",
  "method": "CreditCard",
  "gateway": "Stripe",
  "cardHolderName": "John Doe",
  "cardToken": "tok_...",
  "cardExpiryMonth": "12",
  "cardExpiryYear": "2025",
  "billingEmail": "john@example.com",
  "payerIpAddress": "192.168.1.1"
}

Response: 201 Created
{
  "id": 1,
  "orderId": 123,
  "amount": 99.99,
  "currency": "USD",
  "status": "Processing",
  "gateway": "Stripe",
  "transactionId": "pi_1234567890"
}
```

#### Confirm Payment
```json
POST /api/payment/confirm
{
  "paymentId": 1,
  "stripePaymentIntentId": "pi_1234567890",
  "confirmationToken": "token_xyz",
  "cvc": "123"
}

Response: 200 OK
{
  "id": 1,
  "status": "Completed",
  "transactionId": "pi_1234567890",
  "paymentDate": "2026-02-25T10:30:00Z"
}
```

#### Refund Payment
```json
POST /api/payment/refund
{
  "paymentId": 1,
  "refundAmount": 50.00,
  "reason": "Customer requested partial refund"
}

Response: 200 OK
{
  "id": 1,
  "status": "Refunded",
  "refundedAmount": 50.00,
  "refundDate": "2026-02-25T10:35:00Z"
}
```

## Payment Gateway Integration

### Stripe Integration

**Features:**
- Create payment intents
- Confirm and charge payments
- Refund transactions
- Webhook event processing
- Signature verification

**Configuration:**
```json
"Stripe": {
  "SecretKey": "sk_test_...",
  "PublishableKey": "pk_test_...",
  "WebhookSecret": "whsec_..."
}
```

**Webhook Events Handled:**
- `payment_intent.succeeded` - Payment successful
- `charge.refunded` - Refund processed
- `charge.failed` - Payment failed

### PayPal Integration

**Features:**
- Create orders
- Capture authorized payments
- Refund transactions
- Order status queries
- Webhook event processing

**Configuration:**
```json
"PayPal": {
  "ClientId": "...",
  "Secret": "...",
  "Mode": "sandbox|live",
  "WebhookId": "..."
}
```

**Webhook Events Handled:**
- `CHECKOUT.ORDER.COMPLETED` - Order completed
- `PAYMENT.CAPTURE.COMPLETED` - Payment captured
- `PAYMENT.CAPTURE.REFUNDED` - Refund processed

### PaymentGatewayFactory

Centralized factory for creating gateway instances:
```csharp
var factory = serviceProvider.GetRequiredService<IPaymentGatewayFactory>();
var gateway = factory.CreateGateway(PaymentGateway.Stripe);
// or
var defaultGateway = factory.GetDefaultGateway();
```

## Security & Compliance

### PCI DSS Compliance

1. **Card Data Handling:**
   - Never store full card numbers in plain text
   - Use tokenization (Stripe/PayPal tokens)
   - Encrypt card data at rest
   - Only store last 4 digits for display

2. **HTTPS/TLS:**
   - All API endpoints require HTTPS
   - SSL/TLS 1.2+ enforced

3. **Authentication & Authorization:**
   - JWT bearer token validation
   - User context verification
   - Payment ownership checks

4. **Webhook Security:**
   - Signature verification for all webhooks
   - Webhook source validation
   - Idempotency handling

### Data Protection

- Soft deletes for audit trail
- Encrypted database columns for sensitive data
- Audit logging of all payment events
- Rate limiting on payment endpoints (configurable)

## Database Schema

### Tables

#### Payments
- Id (Primary Key)
- OrderId (Index)
- UserId (Index)
- Amount (Decimal 18,2)
- Currency (VARCHAR 3)
- Status (ENUM)
- Method (ENUM)
- Gateway (ENUM)
- TransactionId (UNIQUE, Index)
- CardLast4 (Encrypted)
- CardHolderName (Encrypted)
- CardExpiryMonth (Encrypted)
- CardExpiryYear (Encrypted)
- PaymentDate
- RefundedAmount
- RefundDate
- ErrorMessage
- GatewayResponseData (NVARCHAR(MAX))
- PayerIpAddress
- CreatedAt (Index)
- UpdatedAt
- CreatedBy
- UpdatedBy
- IsDeleted
- DeletedAt

#### PaymentRefunds
- Id (Primary Key)
- PaymentId (Foreign Key, Index)
- RefundAmount (Decimal 18,2)
- Reason
- Status (ENUM)
- ProcessedDate
- GatewayRefundId
- ErrorMessage
- CreatedAt (Index)
- UpdatedAt
- CreatedBy
- UpdatedBy
- IsDeleted
- DeletedAt

#### PaymentLogs
- Id (Primary Key)
- PaymentId (Foreign Key, Index)
- TransactionId (Index)
- EventType (Index)
- Status (ENUM)
- Message
- ErrorMessage
- ResponseData (NVARCHAR(MAX))
- Metadata (NVARCHAR(MAX))
- Timestamp (Index)
- CreatedAt
- UpdatedAt
- CreatedBy
- UpdatedBy
- IsDeleted
- DeletedAt

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "PaymentDb": "Server=...;Database=Mango_Payment;..."
  },
  "PaymentGateway": {
    "Default": "Stripe",
    "Stripe": {
      "SecretKey": "sk_test_...",
      "PublishableKey": "pk_test_...",
      "WebhookSecret": "whsec_...",
      "ApiVersion": "2024-04-10"
    },
    "PayPal": {
      "ClientId": "...",
      "Secret": "...",
      "Mode": "sandbox",
      "WebhookId": "..."
    },
    "CurrenciesSupported": ["USD", "EUR", "GBP", "CAD", "AUD"],
    "MinimumAmount": 0.50,
    "MaximumAmount": 99999.99
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/payment-service-.txt" } }
    ]
  }
}
```

## Service Implementation

### PaymentService

Main orchestration service:

```csharp
public interface IPaymentService
{
    Task<PaymentDto> InitiatePaymentAsync(PaymentInitiateRequest request);
    Task<PaymentDto> ConfirmPaymentAsync(PaymentConfirmRequest request);
    Task<PaymentDto> RefundPaymentAsync(RefundRequest request);
    Task<PaymentStatusResponse> GetPaymentStatusAsync(int paymentId);
    Task<List<PaymentDto>> GetPaymentHistoryAsync(string userId, int skip, int take);
    Task<bool> ProcessWebhookAsync(PaymentGateway gateway, string body, string signature);
    Task<List<string>> ValidatePaymentAmountAsync(decimal amount, string currency);
}
```

## Event Integration

### Events Published

The Payment Service publishes the following events to RabbitMQ (via MassTransit):

1. **PaymentInitiatedEvent**
   - Triggered when payment process starts
   - Consumed by: Email Service, Order Service

2. **PaymentCompletedEvent**
   - Triggered when payment is successful
   - Consumed by: Order Service, Reward Service, Email Service

3. **PaymentFailedEvent**
   - Triggered when payment fails
   - Consumed by: Order Service, Email Service

4. **PaymentRefundedEvent**
   - Triggered when refund is processed
   - Consumed by: Order Service, Email Service

### Event Structure

```csharp
public class PaymentCompletedEvent
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string TransactionId { get; set; }
    public DateTime CompletedAt { get; set; }
}
```

## Testing

### Unit Tests (22+ Tests)

Located in `tests/Mango.Services.Payment.UnitTests/`

**Payment Entity Tests (14+):**
- InitiatePayment transitions and validation
- CompletePayment state transitions
- FailPayment error handling
- RefundPayment amount validation
- RefundPayment full vs partial
- CanBeRefunded eligibility checks
- CancelPayment state validation
- IsTerminal state checks
- Amount validation within limits
- GetValidationErrors comprehensive checks

**PaymentRefund Entity Tests (8+):**
- ValidateRefund with various amounts
- MarkAsProcessed state transitions
- MarkAsFailed error recording
- GetValidationErrors

### Running Tests

```bash
# Run all tests
dotnet test tests/Mango.Services.Payment.UnitTests/

# Run specific test class
dotnet test tests/Mango.Services.Payment.UnitTests/ --filter ClassName=PaymentTests

# Run with verbose output
dotnet test tests/Mango.Services.Payment.UnitTests/ --verbosity detailed
```

## Running the Service

### Prerequisites

- .NET 10.0 SDK
- SQL Server 2019+ (or LocalDB)
- Stripe account (test keys)
- PayPal account (sandbox)

### Setup

1. **Update appsettings.json:**
   ```json
   {
     "ConnectionStrings": {
       "PaymentDb": "Your SQL Server connection string"
     },
     "PaymentGateway": {
       "Stripe": {
         "SecretKey": "Your Stripe Secret Key",
         "PublishableKey": "Your Stripe Publishable Key",
         "WebhookSecret": "Your Stripe Webhook Secret"
       },
       "PayPal": {
         "ClientId": "Your PayPal Client ID",
         "Secret": "Your PayPal Secret"
       }
     }
   }
   ```

2. **Create Database:**
   ```bash
   # Navigate to API project
   cd src/Payment/API/Mango.Services.Payment.API/

   # Add migration
   dotnet ef migrations add InitialCreate

   # Update database
   dotnet ef database update
   ```

3. **Run Service:**
   ```bash
   dotnet run
   ```

4. **Access API:**
   - Swagger: `https://localhost:5001/openapi/v1.json`
   - Health Check: `https://localhost:5001/health`

## Integration with Order Service

### Payment Flow

1. **Order Created** → Order Service publishes `OrderCreatedEvent`
2. **Payment Initiated** → Payment Service creates payment intent
3. **Payment Confirmed** → Payment Service confirms payment with gateway
4. **Payment Completed** → Payment Service publishes `PaymentCompletedEvent`
5. **Order Updated** → Order Service updates order status to "Shipped"
6. **Reward Awarded** → Reward Service awards loyalty points

### Event Handlers

Order Service subscribes to:
- `PaymentCompletedEvent` → Update order status
- `PaymentFailedEvent` → Update order status, notify customer

Reward Service subscribes to:
- `PaymentCompletedEvent` → Award loyalty points

Email Service subscribes to:
- `PaymentInitiatedEvent` → Send confirmation email
- `PaymentCompletedEvent` → Send receipt
- `PaymentFailedEvent` → Send failure notification
- `PaymentRefundedEvent` → Send refund receipt

## Troubleshooting

### Common Issues

1. **Stripe API Key Invalid**
   - Verify key format (starts with `sk_test_` or `sk_live_`)
   - Check key hasn't been revoked
   - Ensure test/live mode matches your account

2. **PayPal Access Token Expired**
   - Token auto-renews within 60 seconds of expiry
   - Manual refresh: Restart service

3. **Webhook Signature Verification Failed**
   - Verify webhook secret matches gateway settings
   - Check request body hasn't been modified
   - Ensure using correct signature header

4. **Database Connection Issues**
   - Verify SQL Server is running
   - Check connection string in appsettings.json
   - Ensure database exists
   - Run migrations: `dotnet ef database update`

## Performance Considerations

- Database indexes on frequently queried columns
- Async/await throughout for scalability
- Connection pooling via EF Core
- Rate limiting on public endpoints
- Webhook processing off main thread
- Caching of payment gateway configurations

## Security Best Practices

1. Never log full card numbers or CVV
2. Use HTTPS only in production
3. Rotate API keys regularly
4. Monitor for fraud patterns
5. Implement rate limiting
6. Validate all webhook signatures
7. Use separate test and production credentials
8. Regular security audits
9. PCI DSS compliance verification

## Future Enhancements

- [ ] Payment analytics dashboard
- [ ] Advanced fraud detection
- [ ] Multiple currency support with conversion
- [ ] Payment plans/subscriptions
- [ ] Digital wallet integration (Apple Pay, Google Pay)
- [ ] 3D Secure authentication
- [ ] Dispute/chargeback handling
- [ ] Payment reconciliation reports
- [ ] Webhook replay functionality
- [ ] Customer payment methods storage

## Support & Documentation

- API Documentation: See swagger at `/openapi/v1.json`
- Stripe Docs: https://stripe.com/docs
- PayPal Docs: https://developer.paypal.com/
- Code Comments: Comprehensive XML documentation throughout
