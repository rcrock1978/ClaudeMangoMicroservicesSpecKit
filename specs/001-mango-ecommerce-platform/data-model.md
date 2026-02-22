# Data Model: Mango Microservices E-Commerce Platform

**Branch**: `001-mango-ecommerce-platform`
**Date**: 2026-02-23

Each bounded context owns its database. Entities below are grouped by
the service that owns them. Cross-service references use IDs only (no
foreign keys across databases).

---

## Auth Service (MangoAuthDb)

### ApplicationUser

| Field | Type | Constraints |
|-------|------|-------------|
| Id | string (GUID) | PK |
| Name | string(100) | Required |
| Email | string(256) | Required, Unique |
| PasswordHash | string | Required (hashed + salted) |
| PhoneNumber | string(20) | Optional |
| FailedLoginAttempts | int | Default 0 |
| LockoutEndUtc | DateTime? | Null when not locked |
| CreatedAt | DateTime | Required, auto-set |
| UpdatedAt | DateTime | Required, auto-updated |

**Notes**: Extends ASP.NET Core Identity IdentityUser. Password
complexity: min 8 chars, 1 uppercase, 1 number, 1 special char.
Account locks for 15 minutes after 5 consecutive failed logins.

### ApplicationRole

| Field | Type | Constraints |
|-------|------|-------------|
| Id | string (GUID) | PK |
| Name | string(50) | Required, Unique |

**Seed data**: "Customer", "Administrator"

### RefreshToken

| Field | Type | Constraints |
|-------|------|-------------|
| Id | Guid | PK |
| UserId | string | FK → ApplicationUser, Required |
| Token | string | Required, Unique |
| ExpiresUtc | DateTime | Required |
| CreatedAt | DateTime | Required |
| RevokedAt | DateTime? | Null if active |

---

## Product Service (MangoProductDb)

### Category

| Field | Type | Constraints |
|-------|------|-------------|
| CategoryId | int | PK, auto-increment |
| Name | string(100) | Required, Unique |
| CreatedAt | DateTime | Required |
| UpdatedAt | DateTime | Required |

### Product

| Field | Type | Constraints |
|-------|------|-------------|
| ProductId | int | PK, auto-increment |
| Name | string(200) | Required |
| Description | string(1000) | Required |
| Price | decimal(18,2) | Required, > 0 |
| ImageUrl | string(500) | Required |
| CategoryId | int | FK → Category, Required |
| IsAvailable | bool | Default true |
| CreatedAt | DateTime | Required |
| UpdatedAt | DateTime | Required |

**Indexes**: (CategoryId), (Name) for search, (CreatedAt DESC) for
sorting by newest.

**State**: IsAvailable = true/false (simplified inventory per spec).

---

## Shopping Cart Service (MangoShoppingCartDb)

### CartHeader

| Field | Type | Constraints |
|-------|------|-------------|
| CartHeaderId | int | PK, auto-increment |
| UserId | string | Required, Unique (one cart per user) |
| CouponCode | string(50) | Optional |
| Discount | decimal(18,2) | Default 0 |
| CartTotal | decimal(18,2) | Default 0 |
| CreatedAt | DateTime | Required |
| UpdatedAt | DateTime | Required |

### CartDetails

| Field | Type | Constraints |
|-------|------|-------------|
| CartDetailsId | int | PK, auto-increment |
| CartHeaderId | int | FK → CartHeader, Required |
| ProductId | int | Required (cross-service ref) |
| ProductName | string(200) | Denormalized snapshot |
| Price | decimal(18,2) | Denormalized snapshot |
| Count | int | Required, >= 1 |
| CreatedAt | DateTime | Required |
| UpdatedAt | DateTime | Required |

**Notes**: Product details are denormalized into CartDetails to avoid
cross-service calls during cart display. Price is re-validated at
checkout against the Product service.

---

## Coupon Service (MangoCouponDb)

### Coupon

| Field | Type | Constraints |
|-------|------|-------------|
| CouponId | int | PK, auto-increment |
| CouponCode | string(50) | Required, Unique |
| DiscountType | string(20) | Required: "Percentage" or "Fixed" |
| DiscountAmount | decimal(18,2) | Required, > 0 |
| MinAmount | decimal(18,2) | Required, >= 0 |
| ExpirationDate | DateTime | Required |
| IsActive | bool | Default true |
| IsDeleted | bool | Default false (soft delete) |
| CreatedAt | DateTime | Required |
| UpdatedAt | DateTime | Required |
| CreatedBy | string | Required |

---

## Order Service (MangoOrderDb)

### OrderHeader

| Field | Type | Constraints |
|-------|------|-------------|
| OrderHeaderId | int | PK, auto-increment |
| UserId | string | Required |
| CouponCode | string(50) | Optional |
| Discount | decimal(18,2) | Default 0 |
| OrderTotal | decimal(18,2) | Required |
| Status | string(20) | Required, see state machine |
| Name | string(100) | Required (customer name snapshot) |
| Email | string(256) | Required (customer email snapshot) |
| Phone | string(20) | Optional |
| PaymentIntentId | string(100) | Optional (Stripe ref) |
| StripeSessionId | string(100) | Optional |
| OrderTime | DateTime | Required |
| CancelledAt | DateTime? | Null unless cancelled |
| CancelledBy | string? | UserId of who cancelled |
| CreatedAt | DateTime | Required |
| UpdatedAt | DateTime | Required |

**Order Status State Machine**:
```
Pending → Confirmed → Processing → Shipped → Delivered
  ↓           ↓
Cancelled  Cancelled
```
Only transitions along arrows are valid. Cancellation allowed from
Pending or Confirmed only, by customer or administrator.

### OrderDetails

| Field | Type | Constraints |
|-------|------|-------------|
| OrderDetailsId | int | PK, auto-increment |
| OrderHeaderId | int | FK → OrderHeader, Required |
| ProductId | int | Required (cross-service ref) |
| ProductName | string(200) | Snapshot at time of order |
| Price | decimal(18,2) | Snapshot at time of order |
| Count | int | Required, >= 1 |

---

## Email Service (MangoEmailDb)

### EmailLog

| Field | Type | Constraints |
|-------|------|-------------|
| EmailLogId | int | PK, auto-increment |
| RecipientEmail | string(256) | Required |
| Subject | string(500) | Required |
| Body | string(max) | Required |
| TemplateType | string(50) | Required: "Welcome", "OrderConfirmation" |
| Status | string(20) | Required: "Queued", "Sent", "Failed" |
| RetryCount | int | Default 0 |
| SentAt | DateTime? | Null until sent |
| CreatedAt | DateTime | Required |

---

## Reward Service (MangoRewardDb)

### RewardsBalance

| Field | Type | Constraints |
|-------|------|-------------|
| RewardsBalanceId | int | PK, auto-increment |
| UserId | string | Required, Unique |
| TotalPoints | int | Default 0, >= 0 |
| CreatedAt | DateTime | Required |
| UpdatedAt | DateTime | Required |

### RewardsActivity

| Field | Type | Constraints |
|-------|------|-------------|
| RewardsActivityId | int | PK, auto-increment |
| RewardsBalanceId | int | FK → RewardsBalance, Required |
| OrderId | int | Required (cross-service ref) |
| PointsEarned | int | Required, >= 0 |
| OrderTotal | decimal(18,2) | Snapshot for audit |
| ActivityDate | DateTime | Required |

**Calculation**: 1 point per $1 of order value (before discounts),
rounded to nearest whole number.

---

## Cross-Service Reference Map

| Source Service | References | Target Service | Via |
|---------------|------------|----------------|-----|
| ShoppingCart | ProductId | Product | Sync HTTP (gateway) |
| ShoppingCart | CouponCode | Coupon | Sync HTTP (gateway) |
| Order | UserId | Auth | JWT claims |
| Order | ProductId | Product | Snapshot at order time |
| Reward | UserId | Auth | From event payload |
| Reward | OrderId | Order | From event payload |
| Email | UserId/Email | Auth | From event payload |

**Rule**: No cross-database foreign keys. All cross-service references
use value IDs from event payloads or synchronous HTTP lookups.
