# Phase 3: User Story 1 — Browse & Purchase Products (P1 MVP)

**Status**: Ready to Begin
**Prerequisite**: Phase 2 ✅ COMPLETE
**Target Date**: 2026-02-24 to 2026-02-26 (3 days)
**Blocking**: User Stories 2-7

---

## Overview

**User Story 1 MVP** implements the core e-commerce flow:

> **As a customer**, I want to **browse the product catalog** (search, filter, sort), **add items to my cart**, **apply a coupon**, and **check out with payment**, so that I can **purchase products and receive a confirmation email**.

**Success Criteria**:
1. Customer can browse products with search, filtering by category, and sorting (price/name/newest)
2. Customer can add/remove items from shopping cart with quantity management
3. Customer can apply and remove coupons with automatic discount calculation
4. Customer can initiate payment via Stripe checkout
5. Order confirmation email sent upon successful payment
6. Rewards points calculated and credited to account
7. Cart persists across sessions (Redis caching)

---

## Architecture: Microservices Interaction

```
┌─────────────┐
│  Mango.Web  │ (MVC Frontend)
└──────┬──────┘
       │
┌──────▼───────────────┐
│ Ocelot API Gateway   │ (Auth forwarding + routing)
└──────┬───────────────┘
       │
  ┌────┼────┬────────┬─────────┐
  │    │    │        │         │
  ▼    ▼    ▼        ▼         ▼
Product Cart Coupon Order RabbitMQ
 API   API   API     API    (Events)
  │    │    │        │         │
  │    │    │        │    ┌────▼─────┐
  │    │    │        │    │Email API │
  │    │    │        │    │(Consumer)│
  └────┼────┴────────┼────┘──────────┘
       │             │
       ├─────────────┤
       │             │
       ▼             ▼
    Redis      SQL Server
   (Cache)     (Persistent)
```

**Event Flow**:
1. Customer adds items → Cart service
2. Customer applies coupon → Cart validates via Coupon service
3. Customer checks out → Cart publishes `CartCheckoutEvent`
4. Order service receives event → Creates order → Publishes `OrderPlacedEvent`
5. Email service receives event → Sends confirmation email
6. Reward service receives event → Credits points
7. Payment webhook → Order service confirms payment → Publishes `OrderConfirmedEvent`

---

## Database Schema (Phase 3 Focus)

### Product Service
```sql
CREATE TABLE Category (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0
);

CREATE TABLE Product (
    Id INT PRIMARY KEY IDENTITY,
    CategoryId INT FOREIGN KEY REFERENCES Category(Id),
    Name NVARCHAR(200) NOT NULL UNIQUE,
    Description NVARCHAR(1000),
    Price DECIMAL(10, 2) NOT NULL CHECK(Price > 0),
    ImageUrl NVARCHAR(500),
    IsAvailable BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    INDEX idx_category (CategoryId),
    INDEX idx_available (IsAvailable),
    INDEX idx_name (Name)
);
```

### Cart Service
```sql
CREATE TABLE CartHeader (
    CartHeaderId INT PRIMARY KEY IDENTITY,
    UserId NVARCHAR(128) NOT NULL,
    CouponCode NVARCHAR(50),
    Discount DECIMAL(10, 2) DEFAULT 0,
    CartTotal DECIMAL(10, 2) DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX idx_user (UserId)
);

CREATE TABLE CartDetails (
    CartDetailsId INT PRIMARY KEY IDENTITY,
    CartHeaderId INT FOREIGN KEY REFERENCES CartHeader(CartHeaderId) ON DELETE CASCADE,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(200),
    Price DECIMAL(10, 2) NOT NULL,
    Count INT NOT NULL CHECK(Count > 0),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX idx_cart (CartHeaderId),
    INDEX idx_product (ProductId)
);
```

### Coupon Service
```sql
CREATE TABLE Coupon (
    Id INT PRIMARY KEY IDENTITY,
    CouponCode NVARCHAR(50) NOT NULL UNIQUE,
    DiscountAmount DECIMAL(10, 2) NOT NULL CHECK(DiscountAmount >= 0),
    MinAmount INT NOT NULL CHECK(MinAmount >= 0),
    IsActive BIT DEFAULT 1,
    ExpiryDate DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    INDEX idx_code (CouponCode),
    INDEX idx_active (IsActive)
);
```

### Order Service
```sql
CREATE TABLE OrderHeader (
    OrderHeaderId INT PRIMARY KEY IDENTITY,
    UserId NVARCHAR(128) NOT NULL,
    CouponCode NVARCHAR(50),
    Discount DECIMAL(10, 2) DEFAULT 0,
    OrderTotal DECIMAL(10, 2) NOT NULL,
    OrderTotalBeforeDiscount DECIMAL(10, 2),
    OrderStatus NVARCHAR(50) DEFAULT 'Pending', -- Pending|Confirmed|Processing|Shipped|Delivered|Cancelled
    StripePaymentIntentId NVARCHAR(500),
    Name NVARCHAR(200),
    Email NVARCHAR(200),
    Phone NVARCHAR(20),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX idx_user (UserId),
    INDEX idx_status (OrderStatus),
    INDEX idx_payment (StripePaymentIntentId)
);

CREATE TABLE OrderDetails (
    OrderDetailsId INT PRIMARY KEY IDENTITY,
    OrderHeaderId INT FOREIGN KEY REFERENCES OrderHeader(OrderHeaderId) ON DELETE CASCADE,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(200),
    Price DECIMAL(10, 2) NOT NULL,
    Count INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX idx_order (OrderHeaderId)
);
```

### Reward Service
```sql
CREATE TABLE RewardsBalance (
    Id INT PRIMARY KEY IDENTITY,
    UserId NVARCHAR(128) NOT NULL UNIQUE,
    Points INT DEFAULT 0 CHECK(Points >= 0),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE RewardsActivity (
    Id INT PRIMARY KEY IDENTITY,
    UserId NVARCHAR(128) NOT NULL,
    Points INT NOT NULL,
    Activity NVARCHAR(100), -- 'Purchase', 'Refund', 'Adjustment'
    ReferenceId NVARCHAR(100), -- OrderId, etc.
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX idx_user (UserId),
    INDEX idx_reference (ReferenceId)
);
```

---

## API Endpoints (Phase 3 Implementation)

### Product Service - GET endpoints
```
GET /api/v1/products                    # List all with pagination
GET /api/v1/products/search?q=keyword   # Search by name/description
GET /api/v1/products/category/{id}     # Filter by category
GET /api/v1/products/{id}               # Get single product
GET /api/v1/categories                  # List categories
```

**Response Example**:
```json
{
  "isSuccess": true,
  "result": [
    {
      "id": 1,
      "name": "Laptop",
      "price": 999.99,
      "categoryId": 1,
      "categoryName": "Electronics",
      "imageUrl": "https://...",
      "isAvailable": true
    }
  ],
  "message": "Products retrieved successfully",
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 150
}
```

### Cart Service - CRUD operations
```
GET    /api/v1/cart/{userId}                  # Get cart
POST   /api/v1/cart/upsert                    # Add/Update item
DELETE /api/v1/cart/{cartDetailsId}           # Remove item
POST   /api/v1/cart/{userId}/apply-coupon    # Apply coupon
DELETE /api/v1/cart/{userId}/remove-coupon   # Remove coupon
POST   /api/v1/cart/{userId}/checkout        # Initiate checkout
```

### Coupon Service - Validation
```
GET /api/v1/coupons/{code}              # Validate coupon
GET /api/v1/coupons/{code}/discount     # Get discount details
```

### Order Service - Order management
```
GET    /api/v1/orders                        # List customer orders
GET    /api/v1/orders/{orderId}              # Get order details
POST   /api/v1/orders                        # Create order (backend only)
PUT    /api/v1/orders/{orderId}/confirm      # Confirm after payment
PATCH  /api/v1/orders/{orderId}/cancel       # Cancel order
POST   /api/v1/orders/webhook/payment        # Stripe webhook
```

---

## Testing Strategy (TDD Approach)

### Write Tests FIRST ✓

1. **Contract Tests** (Consumer-Driven)
   - Validate API contracts against specification
   - Run before implementation

2. **Unit Tests** (Domain Logic)
   - Entity validation (Product pricing, Cart calculations)
   - Business rules (Coupon eligibility, Discount calculation)

3. **Integration Tests** (End-to-End)
   - Database persistence
   - Repository queries
   - Event publishing/consuming

### Example Test: Coupon Validation
```csharp
[Theory]
[InlineData("SAVE10", 100, true)]       // Valid: coupon applies to $100+
[InlineData("SAVE10", 50, false)]       // Invalid: minimum not met
[InlineData("EXPIRED", 100, false)]     // Invalid: coupon expired
public void ValidateCoupon_Returns_Expected_Result(
    string couponCode, decimal cartTotal, bool expected)
{
    var coupon = new Coupon { Code = couponCode, MinAmount = 100, ExpiryDate = DateTime.UtcNow.AddDays(1) };
    var result = coupon.IsValidFor(cartTotal);
    Assert.Equal(expected, result);
}
```

---

## Phase 3 Task Breakdown

### T056-T065: Product Service (Browse)
- T056: Create ProductRepository with search/filter/sort
- T057: Implement ProductService query handlers (MediatR)
- T058: Create ProductController with endpoints
- T059: Implement caching strategy (Redis)
- T060: Write product service unit tests
- T061: Write product service integration tests
- T062: Write product contract tests
- T063: Add Category support
- T064: Database migrations
- T065: Seed sample product data

### T066-T080: Shopping Cart Service
- T066: Implement CartRepository (CRUD)
- T067: Create CartService with cart calculations
- T068: Implement AddToCart/RemoveFromCart/ClearCart commands
- T069: Implement ApplyCoupon/RemoveCoupon commands
- T070: Integrate product price verification
- T071: Cache cart in Redis
- T072: Create CartController endpoints
- T073: Write cart unit tests (calculation logic)
- T074: Write cart integration tests (DB + Redis)
- T075: Write cart contract tests
- T076-T080: Additional cart features (persistence, notifications)

### T081-T090: Coupon Service
- T081: Implement CouponRepository
- T082: Create CouponService with validation
- T083: Implement discount calculation logic
- T084: Create CouponController endpoints
- T085: Cache coupon results
- T086: Write coupon validation unit tests
- T087: Write coupon integration tests
- T088: Write coupon contract tests
- T089-T090: Coupon edge cases (expiry, min amount)

### T091-T110: Order Service
- T091: Implement OrderRepository
- T092: Create Order entity with state machine (Pending→Confirmed→Processing→...)
- T093: Create CreateOrderCommand handler
- T094: Integrate IPaymentService (Stripe)
- T095: Implement checkout flow
- T096: Create OrderPlacedEvent publisher
- T097: Implement webhook handling for payment confirmation
- T098: Create OrderController endpoints
- T099: Order cancellation logic
- T100: Write order service unit tests
- T101: Write order integration tests
- T102: Write order contract tests
- T103-T110: Order features (tracking, notifications, refunds)

### T111-T120: Email Service
- T111: Complete OrderPlacedEventConsumer implementation
- T112: Create email template engine (order confirmation)
- T113: Implement OrderCancelledEventConsumer
- T114: Create email template (cancellation)
- T115: Create UserRegisteredEventConsumer (welcome email)
- T116: Implement EmailService with SMTP/SendGrid
- T117: Write email consumer unit tests
- T118: Write email integration tests
- T119: Email retry logic on failure
- T120: Email template validation

### T121-T130: Reward Service
- T121: Implement RewardsRepository
- T122: Create RewardsService
- T123: Create OrderConfirmedEventConsumer
- T124: Implement points calculation (1 point per $1)
- T125: Create RewardsBalanceController
- T126: Track RewardsActivity
- T127: Write rewards unit tests
- T128: Write rewards integration tests
- T129: Handle order cancellations (refund points)
- T130: Rewards expiry rules

### T131-T140: Integration & E2E
- T131: Contract tests for all services
- T132: End-to-end smoke test (browse→cart→checkout)
- T133: Payment flow verification
- T134: Event flow verification
- T135: Database migration testing
- T136: Redis cache invalidation testing
- T137: Error handling & edge cases
- T138: Load testing (concurrent shoppers)
- T139: Security testing (JWT, CORS, validation)
- T140: Documentation & API spec review

---

## Implementation Sequence (Recommended)

### Day 1: Product & Cart Services
1. Create database entities and migrations
2. Implement repository patterns
3. Create MediatR command/query handlers
4. Implement controllers
5. Write unit tests (TDD)

### Day 2: Coupon & Order Services
1. Create coupon validation logic
2. Implement order state machine
3. Integrate Stripe payment service
4. Implement checkout flow
5. Create event publishers

### Day 3: Email, Rewards & Integration
1. Implement event consumers
2. Create email templates
3. Implement rewards points calculation
4. E2E testing
5. Documentation

---

## Build Verification Checkpoint

Before moving forward:
```bash
cd src
dotnet build
dotnet test
```

**Expected**: ✓ Compilation + All tests pass

---

## Next Phase Preparation

After Phase 3 completion, all infrastructure is in place for:
- **US2**: User authentication & registration
- **US3**: Order history & tracking
- **US4**: User account management
- **US5-US7**: Admin features, gift cards, analytics

---

## Key Learnings & Patterns

### Repository Pattern
```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> SearchAsync(string query, int pageNum, int pageSize);
    Task<List<Product>> GetByCategoryAsync(int categoryId);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
}
```

### MediatR Command Pattern
```csharp
public class AddToCartCommand : BaseCommand<ResponseDto>
{
    public string UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(AddToCartCommand request, CancellationToken ct)
    {
        // Implementation
    }
}
```

### Event-Driven Updates
```csharp
// Order service publishes
await _publishEndpoint.Publish(new OrderPlacedEvent { ... });

// Email service consumes
public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        // Send email
    }
}
```

---

## Success Criteria Checklist

- [ ] All 7 services have complete domain models
- [ ] All repositories implemented and tested
- [ ] All MediatR handlers implemented
- [ ] All API controllers implemented
- [ ] All event consumers working
- [ ] E2E flow tested: Browse→Add→Checkout→Email→Rewards
- [ ] Redis caching validated
- [ ] Stripe integration verified
- [ ] Database migrations applied
- [ ] 90%+ test coverage
- [ ] All contracts passing

---

## Estimated Effort

| Component | Effort | Owner |
|-----------|--------|-------|
| Product Service | 4 hours | - |
| Cart Service | 5 hours | - |
| Coupon Service | 3 hours | - |
| Order Service | 6 hours | - |
| Email Service | 3 hours | - |
| Reward Service | 3 hours | - |
| Integration/Testing | 4 hours | - |
| **Total** | **28 hours** | - |

---

Ready to begin Phase 3? Let's implement the MVP!
