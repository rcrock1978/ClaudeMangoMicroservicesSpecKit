# Complete Checkout Workflow - Phase 3

**Document Version:** 1.0
**Last Updated:** February 25, 2026

## Overview

This document provides a step-by-step walkthrough of the complete user checkout journey in the Mango eCommerce Phase 3 system, including all API calls, request/response payloads, and error scenarios.

## Workflow Overview

```
┌─────────────────────────────────────────────────────────────────┐
│ Step 1: BROWSE PRODUCTS                                         │
│ User views available products with search and pagination        │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ Step 2: ADD TO SHOPPING CART                                    │
│ User selects products and adds them to personal shopping cart   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ Step 3: REVIEW CART & APPLY COUPON                              │
│ User reviews cart, applies discount coupon (optional)           │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ Step 4: CREATE ORDER                                            │
│ User proceeds to checkout and creates order with cart contents  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ Step 5: ORDER CONFIRMATION                                      │
│ System confirms order, sends confirmation email, awards points  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ Step 6: ORDER FULFILLMENT                                       │
│ Order is processed, shipped, and tracking provided             │
└─────────────────────────────────────────────────────────────────┘
```

---

## Step 1: Browse Products

### Scenario 1.1: Get All Products (Paginated)

Users see a list of all available products with pagination support.

#### Request

```http
GET /api/products?pageNumber=1&pageSize=10 HTTP/1.1
Host: localhost:5001
Accept: application/json
```

#### Response (200 OK)

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalItems": 45,
  "totalPages": 5,
  "items": [
    {
      "id": 1,
      "name": "Dell XPS 13 Laptop",
      "description": "Ultraslim 13.4-inch FHD display laptop with Intel Core i7",
      "price": 1299.99,
      "imageUrl": "https://example.com/products/xps-13.jpg",
      "categoryId": 1,
      "isAvailable": true
    },
    {
      "id": 2,
      "name": "Apple Magic Mouse",
      "description": "Wireless multi-touch mouse with rechargeable battery",
      "price": 79.99,
      "imageUrl": "https://example.com/products/magic-mouse.jpg",
      "categoryId": 2,
      "isAvailable": true
    },
    {
      "id": 3,
      "name": "USB-C Hub",
      "description": "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader",
      "price": 49.99,
      "imageUrl": "https://example.com/products/usb-hub.jpg",
      "categoryId": 2,
      "isAvailable": true
    }
  ]
}
```

**What Happens:**
1. Product Service queries the database
2. Returns paginated results with metadata
3. User sees first 10 products of 45 total
4. Navigation available to view other pages

---

### Scenario 1.2: Search Products

User searches for specific products by keyword.

#### Request

```http
GET /api/products/search?searchTerm=laptop&pageNumber=1&pageSize=10 HTTP/1.1
Host: localhost:5001
Accept: application/json
```

#### Response (200 OK)

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalItems": 3,
  "totalPages": 1,
  "items": [
    {
      "id": 1,
      "name": "Dell XPS 13 Laptop",
      "description": "Ultraslim 13.4-inch FHD display laptop with Intel Core i7",
      "price": 1299.99,
      "imageUrl": "https://example.com/products/xps-13.jpg",
      "categoryId": 1,
      "isAvailable": true
    },
    {
      "id": 5,
      "name": "MacBook Pro 14 Laptop",
      "description": "14-inch MacBook Pro with M3 Pro chip",
      "price": 1999.99,
      "imageUrl": "https://example.com/products/macbook-pro.jpg",
      "categoryId": 1,
      "isAvailable": true
    }
  ]
}
```

**What Happens:**
1. Product Service searches products by name/description
2. Returns matching results filtered by search term
3. User sees only relevant products

---

### Scenario 1.3: Get Products by Category

User filters products by category.

#### Request

```http
GET /api/products/category/1?pageNumber=1&pageSize=10 HTTP/1.1
Host: localhost:5001
Accept: application/json
```

#### Response (200 OK)

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalItems": 2,
  "totalPages": 1,
  "items": [
    {
      "id": 1,
      "name": "Dell XPS 13 Laptop",
      "description": "Ultraslim 13.4-inch FHD display laptop with Intel Core i7",
      "price": 1299.99,
      "imageUrl": "https://example.com/products/xps-13.jpg",
      "categoryId": 1,
      "isAvailable": true
    },
    {
      "id": 5,
      "name": "MacBook Pro 14 Laptop",
      "description": "14-inch MacBook Pro with M3 Pro chip",
      "price": 1999.99,
      "imageUrl": "https://example.com/products/macbook-pro.jpg",
      "categoryId": 1,
      "isAvailable": true
    }
  ]
}
```

**What Happens:**
1. Product Service filters by category
2. Returns all products in selected category
3. User can browse by product type

---

### Scenario 1.4: Get Single Product Details

User views detailed information about a specific product.

#### Request

```http
GET /api/products/1 HTTP/1.1
Host: localhost:5001
Accept: application/json
```

#### Response (200 OK)

```json
{
  "id": 1,
  "name": "Dell XPS 13 Laptop",
  "description": "Ultraslim 13.4-inch FHD display laptop with Intel Core i7, 16GB RAM, 512GB SSD",
  "price": 1299.99,
  "imageUrl": "https://example.com/products/xps-13.jpg",
  "categoryId": 1,
  "isAvailable": true,
  "stockQuantity": 50
}
```

**What Happens:**
1. Product Service retrieves product by ID
2. Returns complete product details
3. User sees full specifications and stock status

---

## Step 2: Add Items to Shopping Cart

### Scenario 2.1: Add Single Item to Cart

User adds a product to their shopping cart.

#### Request

```http
POST /api/cart/user123/items HTTP/1.1
Host: localhost:5002
Content-Type: application/json

{
  "productId": 1,
  "productName": "Dell XPS 13 Laptop",
  "price": 1299.99,
  "quantity": 1
}
```

#### Response (200 OK or 201 Created)

```json
{
  "id": "user123",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 1
    }
  ],
  "totalPrice": 1299.99,
  "totalItems": 1
}
```

**What Happens:**
1. Shopping Cart Service validates user ID
2. Creates or updates cart for user
3. Adds item to cart
4. Updates cart totals
5. Returns updated cart

---

### Scenario 2.2: Add Multiple Items to Cart

User adds additional products to existing cart.

#### Request (First Item Already Added)

```http
POST /api/cart/user123/items HTTP/1.1
Host: localhost:5002
Content-Type: application/json

{
  "productId": 2,
  "productName": "Apple Magic Mouse",
  "price": 79.99,
  "quantity": 1
}
```

#### Response (200 OK)

```json
{
  "id": "user123",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 1
    },
    {
      "id": 2,
      "productId": 2,
      "productName": "Apple Magic Mouse",
      "price": 79.99,
      "quantity": 1
    }
  ],
  "totalPrice": 1379.98,
  "totalItems": 2
}
```

**What Happens:**
1. Shopping Cart Service finds existing cart
2. Adds new item to cart
3. Recalculates totals
4. Returns updated cart with both items

---

### Scenario 2.3: Update Item Quantity

User increases quantity of item in cart.

#### Request

```http
PUT /api/cart/user123/items/1 HTTP/1.1
Host: localhost:5002
Content-Type: application/json

{
  "quantity": 3
}
```

#### Response (200 OK)

```json
{
  "id": "user123",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 3
    },
    {
      "id": 2,
      "productId": 2,
      "productName": "Apple Magic Mouse",
      "price": 79.99,
      "quantity": 1
    }
  ],
  "totalPrice": 4019.95,
  "totalItems": 4
}
```

**What Happens:**
1. Shopping Cart Service finds item in cart
2. Updates quantity to 3
3. Recalculates cart totals
4. Returns updated cart

---

### Scenario 2.4: Remove Item from Cart

User removes an item from shopping cart.

#### Request

```http
DELETE /api/cart/user123/items/2 HTTP/1.1
Host: localhost:5002
```

#### Response (200 OK or 204 No Content)

```json
{
  "id": "user123",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 3
    }
  ],
  "totalPrice": 3899.97,
  "totalItems": 3
}
```

**What Happens:**
1. Shopping Cart Service finds item
2. Removes item from cart
3. Recalculates totals
4. Returns updated cart without removed item

---

## Step 3: Review Cart & Apply Coupon

### Scenario 3.1: Get Current Cart

User reviews their shopping cart before checkout.

#### Request

```http
GET /api/cart/user123 HTTP/1.1
Host: localhost:5002
Accept: application/json
```

#### Response (200 OK)

```json
{
  "id": "user123",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 3
    }
  ],
  "totalPrice": 3899.97,
  "totalItems": 3
}
```

**What Happens:**
1. Shopping Cart Service retrieves user's cart
2. Returns current cart contents and totals
3. User can review before proceeding

---

### Scenario 3.2: Validate Coupon with Discount

User applies a discount coupon to their cart.

#### Request

```http
GET /api/coupon/validate?code=SAVE10&cartTotal=3899.97 HTTP/1.1
Host: localhost:5004
Accept: application/json
```

#### Response (200 OK)

```json
{
  "couponCode": "SAVE10",
  "isValid": true,
  "discountType": "percentage",
  "discountValue": 10,
  "originalTotal": 3899.97,
  "discountAmount": 389.99,
  "finalTotal": 3509.98,
  "message": "Coupon successfully applied"
}
```

**What Happens:**
1. Coupon Service retrieves coupon by code
2. Validates coupon is active and not expired
3. Validates cart total meets minimum requirement
4. Calculates discount amount (10% = $389.99)
5. Returns calculated discount information
6. User sees final price after discount

---

### Scenario 3.3: Attempt Invalid Coupon

User tries to apply an invalid or expired coupon.

#### Request

```http
GET /api/coupon/validate?code=INVALIDCODE&cartTotal=3899.97 HTTP/1.1
Host: localhost:5004
Accept: application/json
```

#### Response (404 Not Found)

```json
{
  "statusCode": 404,
  "message": "Coupon code 'INVALIDCODE' not found or is invalid"
}
```

**What Happens:**
1. Coupon Service searches for coupon
2. Coupon not found in database
3. Returns 404 error
4. User cannot apply invalid coupon

---

### Scenario 3.4: Expired Coupon

User tries to apply an expired coupon.

#### Request

```http
GET /api/coupon/validate?code=EXPIRED10&cartTotal=3899.97 HTTP/1.1
Host: localhost:5004
Accept: application/json
```

#### Response (400 Bad Request)

```json
{
  "statusCode": 400,
  "message": "Coupon has expired",
  "couponCode": "EXPIRED10",
  "expiryDate": "2026-01-31T23:59:59Z"
}
```

**What Happens:**
1. Coupon Service finds coupon
2. Checks expiry date
3. Coupon is expired
4. Returns 400 error with reason
5. User cannot apply expired coupon

---

## Step 4: Create Order

### Scenario 4.1: Create Order with Discount

User completes checkout and creates order with applied coupon.

#### Request

```http
POST /api/orders HTTP/1.1
Host: localhost:5003
Content-Type: application/json

{
  "userId": "user123",
  "cartTotal": 3899.97,
  "discountAmount": 389.99,
  "finalAmount": 3509.98,
  "couponCode": "SAVE10",
  "items": [
    {
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 3
    }
  ]
}
```

#### Response (200 OK or 201 Created)

```json
{
  "id": 1,
  "orderId": "ORD-20260225-001",
  "userId": "user123",
  "cartTotal": 3899.97,
  "discountAmount": 389.99,
  "finalAmount": 3509.98,
  "couponCode": "SAVE10",
  "status": "Pending",
  "items": [
    {
      "id": 1,
      "orderId": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 3
    }
  ],
  "createdAt": "2026-02-25T10:30:00Z"
}
```

**What Happens:**
1. Order Service receives order request
2. Generates unique order ID
3. Saves order to database
4. Saves order items to database
5. **Publishes OrderCreatedEvent to RabbitMQ**
   - Email Service consumer receives event
   - Email Service sends confirmation email
   - Reward Service consumer receives event
   - Reward Service calculates reward points
6. Returns created order

**Asynchronous Actions (via Events):**

```
Order Service                RabbitMQ              Email Service
     │                          │                        │
     ├─ Create Order            │                        │
     │  ├─ Save to DB           │                        │
     │  └─ Publish Event ────────┤                       │
     │                           ├──────────────────────>│
     │                           │                       │
     │                           │                  Send Email:
     │                           │                  Subject: "Order Confirmation - ORD-20260225-001"
     │                           │                  Body: Contains order details
     │                           │                       │
     │                        Reward Service      Log to Email DB
     │                           │                       │
     │                           <──────────────────────┤
     │                           │
     │                      Calculate Points:
     │                      - Base: $3509.98 = 3509.98 points
     │                      - Tier Multiplier: 1.5x (Silver)
     │                      - Total: 5264.97 points
     │                           │
     │                      Save to Reward DB
     │                           │
     └────────────────────────────────────────────────────┘
```

---

### Scenario 4.2: Create Order without Discount

User creates order without applying any coupon.

#### Request

```http
POST /api/orders HTTP/1.1
Host: localhost:5003
Content-Type: application/json

{
  "userId": "user123",
  "cartTotal": 3899.97,
  "discountAmount": 0,
  "finalAmount": 3899.97,
  "couponCode": null,
  "items": [
    {
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 3
    }
  ]
}
```

#### Response (200 OK or 201 Created)

```json
{
  "id": 1,
  "orderId": "ORD-20260225-001",
  "userId": "user123",
  "cartTotal": 3899.97,
  "discountAmount": 0,
  "finalAmount": 3899.97,
  "couponCode": null,
  "status": "Pending",
  "items": [
    {
      "id": 1,
      "orderId": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 3
    }
  ],
  "createdAt": "2026-02-25T10:30:00Z"
}
```

**What Happens:**
1. Order Service receives order request (no coupon)
2. Generates unique order ID
3. Saves order with discount amount = 0
4. Publishes OrderCreatedEvent
5. Services handle event same as before

---

## Step 5: Order Confirmation

### Scenario 5.1: Confirm Order

Administrator or system confirms the order for fulfillment.

#### Request

```http
PUT /api/orders/1/confirm HTTP/1.1
Host: localhost:5003
Content-Type: application/json

{
  "status": "Confirmed"
}
```

#### Response (200 OK)

```json
{
  "id": 1,
  "orderId": "ORD-20260225-001",
  "userId": "user123",
  "cartTotal": 3899.97,
  "discountAmount": 389.99,
  "finalAmount": 3509.98,
  "couponCode": "SAVE10",
  "status": "Confirmed",
  "items": [
    {
      "id": 1,
      "orderId": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 3
    }
  ],
  "createdAt": "2026-02-25T10:30:00Z",
  "confirmedAt": "2026-02-25T10:35:00Z"
}
```

**What Happens:**
1. Order Service finds order by ID
2. Updates status to "Confirmed"
3. Sets confirmationDate
4. Saves to database
5. **Publishes OrderConfirmedEvent**
   - Email Service consumer receives event
   - Email Service sends confirmation email

---

## Step 6: Order Fulfillment

### Scenario 6.1: Ship Order

Order is processed and shipped with tracking information.

#### Request

```http
PUT /api/orders/1/ship HTTP/1.1
Host: localhost:5003
Content-Type: application/json

{
  "status": "Shipped",
  "trackingNumber": "TRACK-1234567890"
}
```

#### Response (200 OK)

```json
{
  "id": 1,
  "orderId": "ORD-20260225-001",
  "userId": "user123",
  "cartTotal": 3899.97,
  "discountAmount": 389.99,
  "finalAmount": 3509.98,
  "couponCode": "SAVE10",
  "status": "Shipped",
  "items": [
    {
      "id": 1,
      "orderId": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 3
    }
  ],
  "createdAt": "2026-02-25T10:30:00Z",
  "confirmedAt": "2026-02-25T10:35:00Z",
  "shippedAt": "2026-02-25T14:00:00Z",
  "trackingNumber": "TRACK-1234567890"
}
```

**What Happens:**
1. Order Service finds order by ID
2. Updates status to "Shipped"
3. Sets tracking number
4. Updates shipment date
5. Saves to database
6. **Publishes OrderShippedEvent**
   - Email Service consumer receives event
   - Email Service sends shipping notification with tracking

---

### Scenario 6.2: Cancel Order

User or admin cancels the order.

#### Request

```http
DELETE /api/orders/1 HTTP/1.1
Host: localhost:5003
```

#### Response (200 OK or 204 No Content)

```json
{
  "id": 1,
  "orderId": "ORD-20260225-001",
  "userId": "user123",
  "status": "Cancelled",
  "cancelledAt": "2026-02-25T11:00:00Z",
  "message": "Order has been cancelled successfully"
}
```

**What Happens:**
1. Order Service finds order
2. Checks if order can be cancelled
3. Updates status to "Cancelled"
4. Saves to database
5. **Publishes OrderCancelledEvent**
   - Email Service sends cancellation notification
   - Reward Service reverses earned points

---

## Error Scenarios

### Error 1: Product Out of Stock

User adds out-of-stock product to cart.

#### Response (400 Bad Request)

```json
{
  "statusCode": 400,
  "message": "Product is out of stock",
  "productId": 5,
  "productName": "Sony WH-1000XM5 Headphones",
  "availableQuantity": 0
}
```

**What Happens:**
1. Shopping Cart Service validates product availability
2. Product is out of stock
3. Request rejected
4. User sees error message

---

### Error 2: Invalid User Session

Request made with invalid user ID.

#### Response (401 Unauthorized)

```json
{
  "statusCode": 401,
  "message": "Invalid user session",
  "details": "User ID could not be verified"
}
```

**What Happens:**
1. Service validates user authentication
2. User session is invalid or expired
3. Request rejected
4. User needs to re-authenticate

---

### Error 3: Insufficient Funds (Future)

When payment processing is implemented:

#### Response (402 Payment Required)

```json
{
  "statusCode": 402,
  "message": "Insufficient funds in payment method",
  "amount": 3509.98,
  "availableFunds": 2000.00
}
```

---

## Complete Transaction Summary

### Total Order Flow Time: 5-15 seconds

```
T+0s   ├─ User submits order
       │
T+1s   ├─ Order Service creates order (200ms)
       ├─ Database save (150ms)
       ├─ Publish OrderCreatedEvent (50ms)
       │
T+2s   ├─ Email Service consumes event (100ms)
       ├─ Generate confirmation email (200ms)
       ├─ Send email (500-2000ms)
       │
T+3s   ├─ Reward Service consumes event (100ms)
       ├─ Calculate reward points (150ms)
       ├─ Update reward database (100ms)
       │
T+5s   └─ All async operations complete
         User receives confirmation on screen
         Confirmation email arrives (within 5 minutes typically)
```

## State Transitions

```
Order Lifecycle:

    [Pending] ──────┬─────────────────────────────────────────┐
                    │                                           │
                    ▼                                           ▼
            [Confirmed]                                    [Cancelled]
                    │                                           │
                    ▼                                           │
            [Processing]                                        │
                    │                                           │
                    ▼                                           │
            [Shipped]                                           │
                    │                                           │
                    ▼                                           ▼
            [Delivered]                               [Refund Processed]
```

## Performance Metrics

| Step | Operation | Duration | Service |
|------|-----------|----------|---------|
| 1 | Browse Products | 100-200ms | Product |
| 2 | Add to Cart | 50-100ms | Cart |
| 3 | Validate Coupon | 30-60ms | Coupon |
| 4 | Create Order | 100-150ms | Order |
| 5 | Confirm Order | 50-100ms | Order |
| 6 | Ship Order | 50-100ms | Order |

**Total Synchronous Time:** ~400-700ms
**Asynchronous Email/Reward Time:** ~1-2 seconds

---

## Conclusion

The complete checkout workflow provides a seamless user experience with proper validation, error handling, and asynchronous event processing. Users see immediate feedback on screen while backend services handle notifications and reward point calculations.

---

*Last Updated: February 25, 2026*
*For API reference, see API_REFERENCE.md*
