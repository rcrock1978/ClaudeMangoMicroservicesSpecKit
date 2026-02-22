# Feature Specification: Mango Microservices E-Commerce Platform

**Feature Branch**: `001-mango-ecommerce-platform`
**Created**: 2026-02-23
**Status**: Draft
**Input**: User description: "Mango Microservices is a full-stack e-commerce platform with 9 microservices covering authentication, product catalog, shopping cart, orders, email notifications, coupons, rewards, an API gateway, and a web frontend. Uses event-driven architecture with message bus, SQL Server persistence, and containerized deployment."

## Clarifications

### Session 2026-02-23

- Q: What are the valid order status transitions? → A: Strict linear flow: Pending → Confirmed → Processing → Shipped → Delivered. Cancellation allowed only from Pending or Confirmed.
- Q: What is the rewards points conversion rate? → A: 1 point per $1 spent (1:1 ratio, rounded to nearest whole number), calculated on order value before discounts.
- Q: What are the password complexity and account lockout requirements? → A: Minimum 8 characters with at least 1 uppercase letter, 1 number, and 1 special character. Account locked for 15 minutes after 5 consecutive failed login attempts.
- Q: Who can initiate an order cancellation? → A: Both the customer who placed the order and any administrator can cancel, but only while the order is in Pending or Confirmed status.
- Q: What product search and sorting capabilities are required? → A: Category filtering, keyword search on product name and description, and sorting by price, name, and newest.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse and Purchase Products (Priority: P1)

A customer visits the Mango storefront, browses the product catalog by
category, adds items to their shopping cart, applies a coupon code for
a discount, and completes checkout to place an order. The customer
receives an email confirmation with order details.

**Why this priority**: This is the core e-commerce flow. Without the
ability to browse, cart, and purchase, the platform has no business
value. Every other feature depends on this end-to-end journey working.

**Independent Test**: Can be fully tested by navigating the storefront,
adding a product to the cart, applying a coupon, checking out, and
verifying the order appears in order history with a confirmation email
sent.

**Acceptance Scenarios**:

1. **Given** a customer is on the storefront, **When** they browse
   products by category, keyword search, or sorting option, **Then**
   they see a paginated list of matching products with name, price,
   description, and image.
2. **Given** a customer views a product, **When** they click
   "Add to Cart", **Then** the item is added to their persistent
   shopping cart with the correct quantity and price.
3. **Given** a customer has items in their cart, **When** they enter
   a valid coupon code, **Then** the discount is applied and the cart
   total is recalculated accordingly.
4. **Given** a customer has items in their cart, **When** they proceed
   to checkout and confirm payment, **Then** an order is created, the
   cart is cleared, and an email confirmation is sent.
5. **Given** a customer enters an invalid coupon code, **When** they
   attempt to apply it, **Then** the system displays a clear error
   message and the cart total remains unchanged.

---

### User Story 2 - User Registration and Authentication (Priority: P2)

A new visitor registers for an account by providing their name, email,
and password. After registration, they can log in to access personalized
features such as order history and saved cart. Administrators can manage
user roles to control access to admin-level operations.

**Why this priority**: Authentication is foundational infrastructure
that gates access to personalized features (order history, rewards,
saved carts). It must be in place before users can have persistent
sessions, but the catalog can be browsed without it.

**Independent Test**: Can be tested by registering a new account,
logging in, verifying the session persists across page navigation,
and confirming an admin can assign roles to users.

**Acceptance Scenarios**:

1. **Given** a visitor is on the registration page, **When** they
   submit valid name, email, and password, **Then** an account is
   created and they are logged in automatically.
2. **Given** a registered user is on the login page, **When** they
   enter valid credentials, **Then** they are authenticated and
   redirected to the storefront with a personalized greeting.
3. **Given** a user is logged in, **When** they navigate between
   pages, **Then** their session persists and they remain
   authenticated.
4. **Given** a user enters an incorrect password, **When** they
   attempt to log in, **Then** the system displays a generic error
   message without revealing whether the email exists.
5. **Given** an administrator, **When** they assign a role to a
   user, **Then** that user gains access to the permissions
   associated with that role.

---

### User Story 3 - Order Management and History (Priority: P3)

An authenticated customer views their complete order history with
status tracking. They can see details of each order including items,
quantities, prices, discounts applied, and current order status.
The system processes orders asynchronously and updates status as
the order progresses through fulfillment stages.

**Why this priority**: Order history and status tracking are essential
for customer trust and reducing support burden, but they build on top
of the purchase flow (P1) and authentication (P2).

**Independent Test**: Can be tested by placing an order, navigating
to order history, verifying the order appears with correct details,
and confirming status updates are reflected in real-time.

**Acceptance Scenarios**:

1. **Given** an authenticated customer with past orders, **When**
   they navigate to "My Orders", **Then** they see a chronological
   list of all their orders with date, total, and status.
2. **Given** a customer viewing order history, **When** they click
   on a specific order, **Then** they see full details including
   line items, quantities, unit prices, discounts, and payment info.
3. **Given** an order has been placed, **When** the order status
   changes during processing, **Then** the customer sees the
   updated status on their next visit to order history.

---

### User Story 4 - Loyalty Rewards Program (Priority: P4)

An authenticated customer earns loyalty points on every purchase.
Points accumulate in their rewards balance and can be viewed on
their profile. The rewards program encourages repeat purchases by
providing a visible points balance and transaction history.

**Why this priority**: The rewards program adds retention value but
is not essential for the core purchase flow. It enhances the platform
after the primary buying journey is complete.

**Independent Test**: Can be tested by placing an order, verifying
points are credited to the customer's rewards balance, and confirming
the points balance is visible on the customer profile.

**Acceptance Scenarios**:

1. **Given** a customer completes a purchase, **When** the order is
   confirmed, **Then** loyalty points proportional to the order value
   are credited to their rewards balance.
2. **Given** an authenticated customer, **When** they view their
   profile, **Then** they see their current rewards points balance.
3. **Given** a customer has earned rewards, **When** they view their
   rewards history, **Then** they see a log of points earned per order.

---

### User Story 5 - Coupon and Promotion Management (Priority: P5)

An administrator creates, updates, and deactivates coupon codes with
configurable discount types (percentage or fixed amount), minimum
purchase thresholds, and expiration dates. Customers can apply these
coupons during checkout.

**Why this priority**: Coupon management is an admin-facing feature
that supports the purchase flow. The customer-facing coupon application
is covered in P1; this story covers the administrative side.

**Independent Test**: Can be tested by an admin creating a coupon with
specific rules, then a customer applying that coupon during checkout
and verifying the discount is correctly calculated.

**Acceptance Scenarios**:

1. **Given** an administrator, **When** they create a new coupon with
   a discount type, value, minimum purchase, and expiration date,
   **Then** the coupon is saved and available for customers to use.
2. **Given** an active coupon exists, **When** a customer applies it
   to a cart meeting the minimum purchase threshold, **Then** the
   correct discount is applied.
3. **Given** an expired or deactivated coupon, **When** a customer
   attempts to apply it, **Then** the system rejects it with a
   clear message explaining why.
4. **Given** an administrator, **When** they deactivate a coupon,
   **Then** it is immediately unavailable for new applications.

---

### User Story 6 - Email Notifications (Priority: P6)

The system sends transactional emails for key events: registration
confirmation, order confirmation with details, and order status
updates. Emails are sent asynchronously so they do not block the
user's workflow.

**Why this priority**: Email notifications enhance the user
experience and provide essential communication, but the platform
is functional without them. They are a polish layer on top of
core flows.

**Independent Test**: Can be tested by triggering each email event
(registration, order placement) and verifying the correct email is
queued and delivered with accurate content.

**Acceptance Scenarios**:

1. **Given** a user registers an account, **When** registration
   succeeds, **Then** a welcome email is sent to their email address.
2. **Given** a customer places an order, **When** the order is
   confirmed, **Then** an order confirmation email with item details
   and total is sent.
3. **Given** the email service is temporarily unavailable, **When**
   an email-triggering event occurs, **Then** the email is queued
   for retry and the user's primary action is not blocked or delayed.

---

### User Story 7 - API Gateway Routing and Frontend Integration (Priority: P7)

All client requests from the web frontend are routed through a single
API gateway that handles request routing to the appropriate backend
service. The gateway provides a unified entry point, simplifying
frontend integration and enabling centralized cross-cutting concerns
such as rate limiting and request aggregation.

**Why this priority**: The gateway is infrastructure that improves
architecture quality but individual services can be called directly
during early development. It becomes critical for production readiness
and security posture.

**Independent Test**: Can be tested by sending requests through the
gateway and verifying they are correctly routed to the target service
and responses are returned to the client.

**Acceptance Scenarios**:

1. **Given** the gateway is running, **When** a client sends a request
   to a gateway route, **Then** the request is forwarded to the correct
   backend service and the response is returned to the client.
2. **Given** a backend service is unavailable, **When** a client sends
   a request through the gateway, **Then** the gateway returns an
   appropriate error response without exposing internal service details.
3. **Given** a request requires authentication, **When** it passes
   through the gateway, **Then** the authentication token is validated
   before routing to the backend service.

---

### Edge Cases

- What happens when a customer adds an out-of-stock product to
  their cart? The system MUST prevent adding unavailable items and
  display a clear out-of-stock message.
- What happens when a customer's cart contains items whose prices
  changed since they were added? The system MUST recalculate totals
  at checkout and notify the customer of price changes.
- What happens when two customers simultaneously attempt to apply
  the last available use of a limited-quantity coupon? The system
  MUST handle concurrency and only honor the coupon for one customer.
- How does the system handle a payment failure during checkout? The
  order MUST NOT be created, the cart MUST be preserved, and the
  customer MUST see an actionable error message.
- What happens when the message bus is temporarily unavailable?
  Critical operations (order creation) MUST still succeed locally,
  and events MUST be queued for later delivery when connectivity
  is restored.
- What happens when a user registers with an email that already
  exists? The system MUST reject the registration with a generic
  message that does not confirm whether the email is already in use.

## Requirements *(mandatory)*

### Functional Requirements

**Authentication & Authorization**

- **FR-001**: System MUST allow visitors to register with name, email, and password.
- **FR-002**: System MUST authenticate users via email and password and issue a session token.
- **FR-003**: System MUST support role-based access control with at least Customer and Administrator roles.
- **FR-004**: System MUST enforce token expiration and require re-authentication after token expiry.
- **FR-005**: System MUST hash and salt all passwords before storage. Passwords MUST be at least 8 characters and contain at least 1 uppercase letter, 1 number, and 1 special character.
- **FR-005a**: System MUST lock a user account for 15 minutes after 5 consecutive failed login attempts. The lockout message MUST NOT reveal whether the account exists.

**Product Catalog**

- **FR-006**: System MUST allow administrators to create, read, update, and delete products.
- **FR-007**: Each product MUST have a name, description, price, category, and image URL.
- **FR-008**: System MUST allow customers to browse products with filtering by category and keyword search on product name and description.
- **FR-008a**: System MUST allow customers to sort product listings by price (low-to-high, high-to-low), name (alphabetical), and newest (most recently added).
- **FR-009**: System MUST support paginated product listings.

**Shopping Cart**

- **FR-010**: System MUST allow authenticated customers to add, update quantity, and remove products from their cart.
- **FR-011**: Cart contents MUST persist across user sessions (server-side storage).
- **FR-012**: System MUST recalculate cart totals when items are added, removed, or modified.
- **FR-013**: System MUST validate product availability before adding to cart.

**Coupon & Promotions**

- **FR-014**: System MUST allow administrators to create coupons with discount type (percentage or fixed), value, minimum purchase amount, and expiration date.
- **FR-015**: System MUST allow customers to apply a coupon code to their cart during checkout.
- **FR-016**: System MUST validate coupon eligibility (active, not expired, minimum purchase met) before applying.
- **FR-017**: System MUST prevent applying multiple coupons to a single order (one coupon per order).

**Order Processing**

- **FR-018**: System MUST create an order from the current cart contents when checkout is confirmed.
- **FR-019**: Each order MUST capture line items, quantities, unit prices, applied discount, and total.
- **FR-020**: System MUST assign an order status (Pending, Confirmed, Processing, Shipped, Delivered, Cancelled). Valid transitions follow a strict linear flow: Pending → Confirmed → Processing → Shipped → Delivered. Cancellation is permitted only from Pending or Confirmed status. No other transitions are allowed.
- **FR-020a**: Both the customer who placed the order and any administrator MUST be able to cancel an order, but only while it is in Pending or Confirmed status. Orders in Processing, Shipped, or Delivered status MUST NOT be cancellable.
- **FR-021**: System MUST provide authenticated customers with access to their full order history.
- **FR-022**: System MUST integrate with a payment provider to process payments during checkout.

**Email Notifications**

- **FR-023**: System MUST send a welcome email upon successful registration.
- **FR-024**: System MUST send an order confirmation email with order details upon successful checkout.
- **FR-025**: Email sending MUST be asynchronous and MUST NOT block the triggering user action.

**Rewards Program**

- **FR-026**: System MUST credit loyalty points to a customer's account upon order completion.
- **FR-027**: System MUST display the customer's current rewards points balance on their profile.
- **FR-028**: Points earned MUST be awarded at a rate of 1 point per $1 of order value (before discounts), rounded to the nearest whole number.

**API Gateway**

- **FR-029**: All client-facing requests MUST be routed through a single API gateway.
- **FR-030**: Gateway MUST authenticate requests before routing to backend services.
- **FR-031**: Gateway MUST return user-friendly error responses when backend services are unavailable.

**Event-Driven Communication**

- **FR-032**: State-changing cross-service events (order placed, user registered) MUST be published to a message bus.
- **FR-033**: Consuming services MUST process messages idempotently (duplicate delivery MUST NOT cause duplicate side effects).
- **FR-034**: Failed message deliveries MUST be retried with backoff and ultimately dead-lettered for investigation.

### Key Entities

- **User**: Represents a registered platform user. Has name, email, hashed password, role(s), and registration date. A user can have one shopping cart, many orders, and a rewards balance.
- **Product**: Represents a purchasable item. Has name, description, price, category, and image URL. Belongs to one category.
- **Category**: Groups products into browsable classifications. Has a name.
- **ShoppingCart**: Represents a customer's in-progress collection of products. Contains cart items (product reference, quantity). Belongs to one user.
- **CartItem**: A line within a shopping cart. References a product with a quantity.
- **Coupon**: Represents a promotional discount. Has a code, discount type (percentage/fixed), discount value, minimum purchase amount, expiration date, and active status.
- **Order**: Represents a completed purchase. Has order date, status, line items, subtotal, discount amount, total, and payment reference. Belongs to one user.
- **OrderItem**: A line within an order. Captures product snapshot (name, price at time of purchase), quantity, and line total.
- **RewardsBalance**: Tracks a customer's loyalty points. Has current balance and transaction history. Belongs to one user.
- **EmailLog**: Records sent transactional emails. Has recipient, subject, template type, sent timestamp, and delivery status.

## Assumptions

- Payment processing integrates with a third-party provider (e.g., Stripe). The specific provider will be determined during planning, but the system will use a payment abstraction to remain provider-agnostic.
- Product inventory tracking is simplified: products are either available or unavailable (no granular stock counts in v1). Inventory management can be expanded in a future iteration.
- One coupon per order is the limit for v1. Stacking multiple coupons is out of scope.
- Rewards points are earn-only in v1. Redemption of points for discounts is out of scope for this iteration.
- Email templates are pre-defined (registration welcome, order confirmation). Custom template management is out of scope.
- The web frontend is server-rendered (MVC pattern), not a single-page application.
- Guest checkout is not supported in v1; customers must register/log in to purchase.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new customer can register, browse products, add items to cart, apply a coupon, and complete a purchase in under 5 minutes on their first visit.
- **SC-002**: The platform supports at least 500 concurrent users performing browse and purchase operations without noticeable performance degradation.
- **SC-003**: Order confirmation emails are delivered within 2 minutes of order placement under normal load.
- **SC-004**: 95% of product catalog page loads complete within 2 seconds.
- **SC-005**: All services remain operational and independently deployable; a failure in the email or rewards service does not prevent customers from completing purchases.
- **SC-006**: Every service exposes health status, and the platform's overall health is monitorable from a single dashboard.
- **SC-007**: Administrator can create a new coupon and have it available for customer use within 1 minute.
- **SC-008**: Loyalty points are credited to the customer's balance within 30 seconds of order confirmation.
- **SC-009**: The platform recovers from a single service failure within 60 seconds through automatic restart, without data loss.
- **SC-010**: 90% of first-time users successfully complete a purchase without requiring support assistance.
