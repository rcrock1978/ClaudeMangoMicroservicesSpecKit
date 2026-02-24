# Integration Guide - Phase 3 Microservices

**Document Version:** 1.0
**Last Updated:** February 25, 2026

## Overview

This document describes the architecture and integration patterns used in Phase 3 of the Mango eCommerce Platform. It covers how the six core microservices interact, communicate, and work together to deliver the complete checkout experience.

## Architecture Overview

### System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                       Client Applications                        │
│                    (Web, Mobile, Desktop)                        │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Ocelot API Gateway                            │
│              (Future: Route aggregation & auth)                  │
└─────────────────────────────────────────────────────────────────┘
           │         │         │         │         │         │
    ▼      ▼      ▼      ▼      ▼      ▼
┌──────┐┌──────┐┌──────┐┌──────┐┌──────┐┌──────┐
│Prod. ││Cart  ││Order ││Coupon││Email ││Reward│
│ Svc  ││ Svc  ││ Svc  ││ Svc  ││ Svc  ││ Svc  │
└──────┘└──────┘└──────┘└──────┘└──────┘└──────┘
   │       │       │       │       │       │
   └───────┴───────┴───┬───┴───────┴───────┘
                       │
                    ▼  ▼
                ┌─────────────┐
                │  RabbitMQ   │
                │ (Events)    │
                └─────────────┘
                       │
         ┌─────────────┼─────────────┐
         │             │             │
    ▼    ▼         ▼    ▼       ▼    ▼
 ┌──────────┐  ┌──────────┐  ┌──────────┐
 │  Email   │  │ Reward   │  │  Product │
 │ Consumer │  │ Consumer │  │ Consumer │
 └──────────┘  └──────────┘  └──────────┘
```

## Microservices Detail

### 1. Product Service

**Port:** 5001
**Database:** Mango_Products
**Purpose:** Manage product catalog, categories, and product browsing

#### Key Endpoints

```http
GET  /api/products                                # Get paginated products
GET  /api/products/{id}                          # Get single product
GET  /api/products/search?searchTerm=...         # Search products
GET  /api/products/category/{categoryId}         # Get category products
```

#### Domain Entities

```csharp
public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }
}

public class Category : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<Product> Products { get; set; }
}
```

#### Events Published

- `ProductUpdatedEvent` - When product details change
- `ProductDeletedEvent` - When product is removed

#### External Dependencies

- None (independent service)

---

### 2. Shopping Cart Service

**Port:** 5002
**Database:** Mango_ShoppingCart
**Purpose:** Manage shopping carts and line items

#### Key Endpoints

```http
GET    /api/cart/{userId}                        # Get user's cart
POST   /api/cart/{userId}/items                  # Add item to cart
PUT    /api/cart/{userId}/items/{productId}     # Update item quantity
DELETE /api/cart/{userId}/items/{productId}     # Remove item
DELETE /api/cart/{userId}                        # Clear entire cart
```

#### Domain Entities

```csharp
public class CartItem : BaseEntity
{
    public string UserId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; }
}

public class Cart : BaseEntity
{
    public string UserId { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; }
    public ICollection<CartItem> Items { get; set; }
}
```

#### Events Published

- `CartCheckoutEvent` - When user proceeds to checkout
- `CartClearedEvent` - When cart is emptied

#### External Dependencies

- Calls Product Service for product validation (optional)

---

### 3. Coupon Service

**Port:** 5004
**Database:** Mango_Coupon
**Purpose:** Manage discount coupons and validation

#### Key Endpoints

```http
GET /api/coupon/validate?code=...&cartTotal=...  # Validate coupon
```

#### Domain Entities

```csharp
public class Coupon : BaseEntity
{
    public string Code { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal MinimumCartTotal { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public int MaxUsageCount { get; set; }
    public int UsageCount { get; set; }
}

public class CouponValidation : BaseEntity
{
    public int CouponId { get; set; }
    public string UserId { get; set; }
    public decimal CartTotal { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTime ValidatedAt { get; set; }
}
```

#### Events Published

- `CouponUsedEvent` - When coupon is applied to order
- `CouponExpiredEvent` - When coupon expires

#### External Dependencies

- None (independent service)

#### Discount Calculation Logic

```csharp
public decimal CalculateDiscount(Coupon coupon, decimal cartTotal)
{
    if (!coupon.IsActive || coupon.ExpiryDate < DateTime.UtcNow)
        return 0;

    if (cartTotal < coupon.MinimumCartTotal)
        return 0;

    if (coupon.UsageCount >= coupon.MaxUsageCount)
        return 0;

    if (coupon.DiscountPercentage > 0)
        return (cartTotal * coupon.DiscountPercentage / 100);

    return coupon.DiscountAmount;
}
```

---

### 4. Order Service

**Port:** 5003
**Database:** Mango_Order
**Purpose:** Manage order creation, fulfillment, and lifecycle

#### Key Endpoints

```http
POST   /api/orders                                # Create order
GET    /api/orders/{id}                          # Get order details
GET    /api/orders/user/{userId}                 # Get user's orders
PUT    /api/orders/{id}/confirm                  # Confirm order
PUT    /api/orders/{id}/ship                     # Ship order
DELETE /api/orders/{id}                          # Cancel order
```

#### Domain Entities

```csharp
public class Order : BaseEntity
{
    public string OrderId { get; set; }
    public string UserId { get; set; }
    public decimal CartTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? CouponCode { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string? TrackingNumber { get; set; }
    public ICollection<OrderItem> Items { get; set; }
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6
}

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
```

#### Events Published

```csharp
// When order is created
public class OrderCreatedEvent
{
    public string OrderId { get; set; }
    public string UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

// When order is confirmed
public class OrderConfirmedEvent
{
    public string OrderId { get; set; }
    public string UserId { get; set; }
    public DateTime ConfirmedAt { get; set; }
}

// When order is shipped
public class OrderShippedEvent
{
    public string OrderId { get; set; }
    public string UserId { get; set; }
    public string TrackingNumber { get; set; }
    public DateTime ShippedAt { get; set; }
}

// When order is cancelled
public class OrderCancelledEvent
{
    public string OrderId { get; set; }
    public string UserId { get; set; }
    public DateTime CancelledAt { get; set; }
}
```

#### External Dependencies

- Shopping Cart Service (validates cart contents)
- Coupon Service (validates discount)
- Product Service (validates products)

---

### 5. Email Service

**Port:** 5005
**Database:** Mango_Email
**Purpose:** Handle email notifications for order events

#### Key Endpoints

```http
GET  /api/email/{emailId}                        # Get email log
GET  /api/email/user/{userId}                    # Get user's emails
POST /api/email/send                             # Manual email send
```

#### Domain Entities

```csharp
public class EmailLog : BaseEntity
{
    public string Recipient { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public EmailStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
    public string CorrelationId { get; set; }
}

public class EmailTemplate : BaseEntity
{
    public string Name { get; set; }
    public string Subject { get; set; }
    public string HtmlContent { get; set; }
    public string? PlainTextContent { get; set; }
}

public enum EmailStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Bounced = 4
}
```

#### Events Consumed

```csharp
// Consumes OrderCreatedEvent
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    // Sends order confirmation email
    // Template: "OrderConfirmation"
    // Variables: OrderId, CustomerName, OrderTotal, Items
}

// Consumes OrderConfirmedEvent
public class OrderConfirmedConsumer : IConsumer<OrderConfirmedEvent>
{
    // Sends order confirmed email
    // Template: "OrderConfirmed"
    // Variables: OrderId, ConfirmationDate
}

// Consumes OrderShippedEvent
public class OrderShippedConsumer : IConsumer<OrderShippedEvent>
{
    // Sends shipping notification email
    // Template: "OrderShipped"
    // Variables: OrderId, TrackingNumber, ShippingDate
}
```

#### Email Templates

```
OrderConfirmation
├─ Subject: "Order Confirmation - #{OrderId}"
├─ Body: Contains order details, items, total
└─ Variables: OrderId, UserId, Amount, Items[]

OrderConfirmed
├─ Subject: "Your Order is Confirmed - #{OrderId}"
├─ Body: Order has been confirmed
└─ Variables: OrderId, ConfirmedDate

OrderShipped
├─ Subject: "Your Order has Shipped - #{TrackingNumber}"
├─ Body: Tracking information
└─ Variables: OrderId, TrackingNumber, ShippedDate
```

#### External Dependencies

- RabbitMQ (for event consumption)
- SMTP Server (for email sending)

---

### 6. Reward Service

**Port:** 5006
**Database:** Mango_Reward
**Purpose:** Manage reward points and loyalty tiers

#### Key Endpoints

```http
GET  /api/reward/user/{userId}/points            # Get user reward points
GET  /api/reward/user/{userId}/tier              # Get user tier
POST /api/reward/user/{userId}/redeem            # Redeem points
```

#### Domain Entities

```csharp
public class RewardPoint : BaseEntity
{
    public string UserId { get; set; }
    public decimal PointsEarned { get; set; }
    public decimal PointsUsed { get; set; }
    public decimal CurrentBalance { get; set; }
    public string TransactionType { get; set; }
    public string? OrderId { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class RewardTier : BaseEntity
{
    public string Name { get; set; }
    public decimal MinimumPoints { get; set; }
    public decimal BonusMultiplier { get; set; }
    public string Benefits { get; set; }
}
```

#### Events Consumed

```csharp
// Consumes OrderCreatedEvent
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    // Calculates reward points (1 point per $1 spent)
    // Applies tier bonus if applicable
    // Credits points to user account
}

// Consumes OrderCancelledEvent
public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    // Reverses reward points earned
}
```

#### Reward Calculation Logic

```csharp
public RewardCalculationResult CalculateRewards(
    Order order,
    RewardTier userTier)
{
    var basePoints = order.FinalAmount;  // 1 point per $1
    var bonusMultiplier = userTier?.BonusMultiplier ?? 1.0m;
    var totalPoints = basePoints * bonusMultiplier;

    return new RewardCalculationResult
    {
        BasePoints = basePoints,
        BonusMultiplier = bonusMultiplier,
        TotalPoints = totalPoints,
        Tier = userTier?.Name
    };
}
```

#### External Dependencies

- RabbitMQ (for event consumption)

---

## Event Flow and Communication

### Message Bus (RabbitMQ)

All async communication between services uses RabbitMQ with MassTransit framework.

#### Exchange Configuration

```csharp
public void ConfigureMassTransit(IServiceCollection services)
{
    services.AddMassTransit(busConfigurator =>
    {
        // Configure endpoints
        busConfigurator.SetKebabCaseEndpointNameFormatter();

        // Event consumers
        busConfigurator.AddConsumer<OrderCreatedConsumer>();
        busConfigurator.AddConsumer<OrderConfirmedConsumer>();
        busConfigurator.AddConsumer<OrderShippedConsumer>();
        busConfigurator.AddConsumer<OrderCancelledConsumer>();

        // Configure RabbitMQ
        busConfigurator.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });

            cfg.ConfigureEndpoints(context);
        });
    });
}
```

#### Event Publishing

```csharp
// In Order Service - when order is created
await _bus.Publish(new OrderCreatedEvent
{
    OrderId = order.OrderId,
    UserId = order.UserId,
    Amount = order.FinalAmount,
    CreatedAt = DateTime.UtcNow,
    Items = orderItems
});
```

#### Event Consumption

```csharp
// In Email Service
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var @event = context.Message;

        // Generate email
        var emailLog = new EmailLog
        {
            Recipient = await GetUserEmail(@event.UserId),
            Subject = "Order Confirmation",
            Body = await GenerateEmailBody(@event),
            Status = EmailStatus.Pending,
            CorrelationId = context.CorrelationId.ToString()
        };

        // Send email
        await _emailService.SendAsync(emailLog);

        // Log to database
        await _emailRepository.AddAsync(emailLog);
    }
}
```

### Event Flow Diagram

```
Order Service              RabbitMQ               Email Service
   │                          │                        │
   ├─ Create Order            │                        │
   │  ├─ Save to DB           │                        │
   │  └─ Publish Event ────────┤                       │
   │                           ├─ Route to Consumers   │
   │                           │                       │
   │                           ├──────────────────────>│
   │                           │                       │
   │                           │                    Send Email
   │                           │                       │
   │                           │    <─ Ack Consumed──┤
   │                           │

Reward Service
   │
   │<─ Order Created Event ────┤
   │                           │
   └─ Calculate & Credit Points
      └─ Save to DB
```

## Data Flow - Complete Checkout Journey

### Step 1: User Browses Products

```
Client
  │
  └─ GET /api/products?pageNumber=1&pageSize=10
              │
              ▼
        Product Service
              │
              ├─ Query Products from DB
              │
              └─ Return PaginatedProductResponse
```

### Step 2: User Adds to Cart

```
Client
  │
  └─ POST /api/cart/{userId}/items
              │
              ▼
        Shopping Cart Service
              │
              ├─ Validate ProductId (optional: call Product Service)
              ├─ Save CartItem to DB
              │
              └─ Return CartDto
```

### Step 3: User Applies Coupon

```
Client
  │
  └─ GET /api/coupon/validate?code=SAVE10&cartTotal=500
              │
              ▼
        Coupon Service
              │
              ├─ Query Coupon from DB
              ├─ Validate:
              │  ├─ Coupon exists
              │  ├─ Not expired
              │  ├─ Cart total meets minimum
              │  └─ Usage limit not reached
              │
              └─ Return DiscountCalculation
```

### Step 4: User Creates Order

```
Client
  │
  └─ POST /api/orders
              │
              ▼
        Order Service
              │
              ├─ Validate request
              ├─ Save Order to DB
              ├─ Save OrderItems to DB
              │
              └─ Publish OrderCreatedEvent ──────────┐
                                                      │
                                          ┌───────────┴────────────┐
                                          │                        │
                                          ▼                        ▼
                                    Email Service              Reward Service
                                          │                        │
                                          ├─ Generate Email        ├─ Calculate Points
                                          ├─ Send Email            ├─ Apply Tier Bonus
                                          └─ Log to DB             └─ Save to DB
```

### Step 5: Order Confirmation

```
Client
  │
  └─ PUT /api/orders/{id}/confirm
              │
              ▼
        Order Service
              │
              ├─ Update Order Status = Confirmed
              ├─ Save to DB
              │
              └─ Publish OrderConfirmedEvent ────────┐
                                                      │
                                          ┌───────────┴────────────┐
                                          │
                                          ▼
                                    Email Service
                                          │
                                          ├─ Generate Confirmation Email
                                          ├─ Send Email
                                          └─ Log to DB
```

### Step 6: Order Shipping

```
Client
  │
  └─ PUT /api/orders/{id}/ship
              │
              ▼
        Order Service
              │
              ├─ Update Order Status = Shipped
              ├─ Set Tracking Number
              ├─ Save to DB
              │
              └─ Publish OrderShippedEvent ───────────┐
                                                       │
                                          ┌────────────┴────────────┐
                                          │
                                          ▼
                                    Email Service
                                          │
                                          ├─ Generate Shipping Email
                                          ├─ Include Tracking Info
                                          ├─ Send Email
                                          └─ Log to DB
```

## Database Schema Overview

### Product Service Schema

```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL,
    ImageUrl NVARCHAR(MAX),
    CategoryId INT NOT NULL,
    IsAvailable BIT NOT NULL,
    StockQuantity INT NOT NULL,
    IsDeleted BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    IsDeleted BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);
```

### Shopping Cart Schema

```sql
CREATE TABLE CartItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(255) NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(255) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    AddedAt DATETIME DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);
```

### Order Service Schema

```sql
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId NVARCHAR(255) UNIQUE NOT NULL,
    UserId NVARCHAR(255) NOT NULL,
    CartTotal DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    FinalAmount DECIMAL(18,2) NOT NULL,
    CouponCode NVARCHAR(50),
    Status INT DEFAULT 1, -- Pending
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    ConfirmedAt DATETIME,
    ShippedAt DATETIME,
    TrackingNumber NVARCHAR(100),
    IsDeleted BIT DEFAULT 0
);

CREATE TABLE OrderItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(255) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);
```

## Integration Testing Strategy

### Test Pyramid

```
         ┌──────────────────────┐
         │    E2E Tests (20)    │  ◄─ Full workflow tests
         │                      │
         ├──────────────────────┤
         │ Integration Tests    │  ◄─ Service-to-service
         │      (20+)           │
         │                      │
         ├──────────────────────┤
         │  Unit Tests (80+)    │  ◄─ Individual components
         │                      │
         └──────────────────────┘
```

### Critical Integration Points

1. **Order Service → Shopping Cart Service**
   - Validates items exist
   - Checks quantities

2. **Order Service → Coupon Service**
   - Validates coupon code
   - Calculates discount

3. **Order Service → Product Service**
   - Confirms product availability
   - Gets current pricing

4. **Order Service → Message Bus**
   - Publishes OrderCreatedEvent
   - Email Service consumes event
   - Reward Service consumes event

## Performance Considerations

### Caching Strategy

```csharp
// Product Service - Cache products
services.AddMemoryCache();

// Cache products for 5 minutes
var cacheOptions = new MemoryCacheEntryOptions()
    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

var cachedProducts = await _cache.GetOrCreateAsync(
    "products_page_1_size_10",
    entry =>
    {
        entry.SetOptions(cacheOptions);
        return _productRepository.GetPaginatedAsync(1, 10);
    });
```

### Database Indexing

```sql
-- Orders table
CREATE INDEX IDX_Orders_UserId ON Orders(UserId);
CREATE INDEX IDX_Orders_OrderId ON Orders(OrderId);
CREATE INDEX IDX_Orders_Status ON Orders(Status);

-- Products table
CREATE INDEX IDX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IDX_Products_IsAvailable ON Products(IsAvailable);

-- CartItems table
CREATE INDEX IDX_CartItems_UserId ON CartItems(UserId);
```

## Security Considerations

### Authentication & Authorization

Currently not implemented in Phase 3 - planned for Phase 4.

### Input Validation

```csharp
// All services validate input
public class CreateOrderRequest
{
    [Required]
    public string UserId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CartTotal { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    [Required]
    public List<OrderItemRequest> Items { get; set; }
}
```

### Data Protection

- All connection strings use encryption
- Passwords hashed with bcrypt
- HTTPS enforced in production
- SQL injection prevented via Entity Framework

## Error Handling

### Standard Error Response

```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "cartTotal",
      "message": "Cart total must be greater than 0"
    }
  ],
  "timestamp": "2026-02-25T10:30:00Z"
}
```

### HTTP Status Codes

| Code | Scenario | Service |
|------|----------|---------|
| 200  | Request successful | All |
| 201  | Resource created | Order, Cart |
| 400  | Invalid request | All |
| 404  | Resource not found | All |
| 500  | Server error | All |
| 503  | Service unavailable | All |

## Deployment Considerations

### Environment Setup

1. **Development**
   - SQL Server Express LocalDB
   - RabbitMQ local instance
   - Services on localhost

2. **Staging**
   - SQL Server Standard Edition
   - RabbitMQ cluster
   - Services in Docker containers

3. **Production**
   - SQL Server Enterprise
   - RabbitMQ with HA
   - Kubernetes orchestration

### Service Health Checks

```csharp
// Add health checks to each service
services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddRabbitMQ(new Uri("amqp://guest:guest@localhost:5672/"));

// Endpoint
app.MapHealthChecks("/health");
```

## Monitoring and Logging

### Structured Logging

```csharp
// Using Serilog
Log.Information("Order created: {OrderId} for user {UserId}",
    orderId, userId);

Log.Error("Failed to send email for order {OrderId}: {Exception}",
    orderId, exception);
```

### Event Tracing

```csharp
// MassTransit correlation IDs
var correlationId = context.CorrelationId;

Log.Information("Consuming OrderCreatedEvent with correlation {CorrelationId}",
    correlationId);
```

## Conclusion

Phase 3 integrates six microservices into a cohesive eCommerce platform. The architecture supports loose coupling through message-based communication, scalability through service isolation, and reliability through comprehensive testing.

---

*Last Updated: February 25, 2026*
*For detailed implementation, see service-specific documentation.*
