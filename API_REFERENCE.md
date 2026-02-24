# Complete API Reference - Phase 3

**Document Version:** 1.0
**Last Updated:** February 25, 2026

## Base URLs

| Service | Base URL |
|---------|----------|
| Product Service | http://localhost:5001/api |
| Shopping Cart Service | http://localhost:5002/api |
| Order Service | http://localhost:5003/api |
| Coupon Service | http://localhost:5004/api |
| Email Service | http://localhost:5005/api |
| Reward Service | http://localhost:5006/api |

## Common Response Format

### Success Response (2xx)

```json
{
  "data": { /* response data */ },
  "statusCode": 200,
  "message": "Operation successful"
}
```

### Error Response (4xx, 5xx)

```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "price",
      "message": "Price must be greater than 0"
    }
  ]
}
```

---

## Product Service API

**Base URL:** `http://localhost:5001/api`

### 1. Get All Products (Paginated)

Retrieve a paginated list of all available products.

```http
GET /products?pageNumber=1&pageSize=10
```

**Query Parameters:**
- `pageNumber` (int, default: 1) - Page number for pagination
- `pageSize` (int, default: 10, max: 100) - Number of items per page

**Response (200 OK):**

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
      "description": "High-performance ultraslim laptop",
      "price": 1299.99,
      "imageUrl": "https://example.com/xps.jpg",
      "categoryId": 1,
      "isAvailable": true
    }
  ]
}
```

**Error Responses:**
- `400 Bad Request` - Invalid page number or size
- `500 Internal Server Error` - Database error

---

### 2. Get Single Product

Retrieve detailed information about a specific product.

```http
GET /products/{id}
```

**Path Parameters:**
- `id` (int) - Product ID

**Response (200 OK):**

```json
{
  "id": 1,
  "name": "Dell XPS 13 Laptop",
  "description": "High-performance ultraslim laptop with Intel Core i7",
  "price": 1299.99,
  "imageUrl": "https://example.com/xps.jpg",
  "categoryId": 1,
  "isAvailable": true,
  "stockQuantity": 50
}
```

**Error Responses:**
- `404 Not Found` - Product doesn't exist
- `500 Internal Server Error` - Database error

---

### 3. Search Products

Search for products by name or description.

```http
GET /products/search?searchTerm=laptop&pageNumber=1&pageSize=10
```

**Query Parameters:**
- `searchTerm` (string, required) - Search keyword
- `pageNumber` (int, default: 1) - Page number
- `pageSize` (int, default: 10, max: 100) - Items per page

**Response (200 OK):**

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
      "price": 1299.99,
      "categoryId": 1,
      "isAvailable": true
    }
  ]
}
```

**Error Responses:**
- `400 Bad Request` - Search term is empty
- `500 Internal Server Error` - Database error

---

### 4. Get Products by Category

Retrieve products filtered by category.

```http
GET /products/category/{categoryId}?pageNumber=1&pageSize=10
```

**Path Parameters:**
- `categoryId` (int) - Category ID

**Query Parameters:**
- `pageNumber` (int, default: 1) - Page number
- `pageSize` (int, default: 10, max: 100) - Items per page

**Response (200 OK):**

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalItems": 5,
  "totalPages": 1,
  "items": [
    {
      "id": 1,
      "name": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "categoryId": 1,
      "isAvailable": true
    }
  ]
}
```

---

## Shopping Cart Service API

**Base URL:** `http://localhost:5002/api`

### 1. Get Shopping Cart

Retrieve the current shopping cart for a user.

```http
GET /cart/{userId}
```

**Path Parameters:**
- `userId` (string) - User identifier

**Response (200 OK):**

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

**Error Responses:**
- `404 Not Found` - Cart doesn't exist (empty cart is still valid)

---

### 2. Add Item to Cart

Add a product to the shopping cart.

```http
POST /cart/{userId}/items
Content-Type: application/json

{
  "productId": 1,
  "productName": "Dell XPS 13 Laptop",
  "price": 1299.99,
  "quantity": 1
}
```

**Path Parameters:**
- `userId` (string) - User identifier

**Request Body:**
- `productId` (int, required) - Product ID
- `productName` (string, required) - Product name
- `price` (decimal, required) - Product price
- `quantity` (int, required) - Quantity to add (min: 1)

**Response (200 OK or 201 Created):**

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

**Error Responses:**
- `400 Bad Request` - Invalid product or quantity
- `404 Not Found` - Product unavailable

---

### 3. Update Cart Item Quantity

Update the quantity of an item in the cart.

```http
PUT /cart/{userId}/items/{productId}
Content-Type: application/json

{
  "quantity": 5
}
```

**Path Parameters:**
- `userId` (string) - User identifier
- `productId` (int) - Product ID

**Request Body:**
- `quantity` (int, required) - New quantity (min: 1)

**Response (200 OK):**

```json
{
  "id": "user123",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 5
    }
  ],
  "totalPrice": 6499.95,
  "totalItems": 5
}
```

**Error Responses:**
- `400 Bad Request` - Invalid quantity
- `404 Not Found` - Item not found in cart

---

### 4. Remove Item from Cart

Remove a product from the shopping cart.

```http
DELETE /cart/{userId}/items/{productId}
```

**Path Parameters:**
- `userId` (string) - User identifier
- `productId` (int) - Product ID

**Response (200 OK or 204 No Content):**

```json
{
  "id": "user123",
  "items": [],
  "totalPrice": 0,
  "totalItems": 0
}
```

---

### 5. Clear Cart

Remove all items from the shopping cart.

```http
DELETE /cart/{userId}
```

**Path Parameters:**
- `userId` (string) - User identifier

**Response (200 OK or 204 No Content):**

```json
{
  "id": "user123",
  "items": [],
  "totalPrice": 0,
  "totalItems": 0
}
```

---

## Coupon Service API

**Base URL:** `http://localhost:5004/api`

### 1. Validate Coupon

Validate a coupon code and calculate discount.

```http
GET /coupon/validate?code=SAVE10&cartTotal=1000
```

**Query Parameters:**
- `code` (string, required) - Coupon code
- `cartTotal` (decimal, required) - Current cart total

**Response (200 OK):**

```json
{
  "couponCode": "SAVE10",
  "isValid": true,
  "discountType": "percentage",
  "discountValue": 10,
  "originalTotal": 1000,
  "discountAmount": 100,
  "finalTotal": 900,
  "message": "Coupon successfully applied"
}
```

**Error Responses:**
- `404 Not Found` - Coupon code doesn't exist
- `400 Bad Request` - Coupon expired or invalid
  ```json
  {
    "statusCode": 400,
    "message": "Coupon has expired",
    "couponCode": "EXPIRED10"
  }
  ```

---

## Order Service API

**Base URL:** `http://localhost:5003/api`

### 1. Create Order

Create a new order from cart items.

```http
POST /orders
Content-Type: application/json

{
  "userId": "user123",
  "cartTotal": 1299.99,
  "discountAmount": 100,
  "finalAmount": 1199.99,
  "couponCode": "SAVE10",
  "items": [
    {
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 1
    }
  ]
}
```

**Request Body:**
- `userId` (string, required) - User identifier
- `cartTotal` (decimal, required) - Total before discount
- `discountAmount` (decimal, required) - Discount amount (can be 0)
- `finalAmount` (decimal, required) - Final amount after discount
- `couponCode` (string, optional) - Applied coupon code
- `items` (array, required) - Order items

**Response (200 OK or 201 Created):**

```json
{
  "id": 1,
  "orderId": "ORD-20260225-001",
  "userId": "user123",
  "cartTotal": 1299.99,
  "discountAmount": 100,
  "finalAmount": 1199.99,
  "couponCode": "SAVE10",
  "status": "Pending",
  "items": [
    {
      "id": 1,
      "orderId": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 1
    }
  ],
  "createdAt": "2026-02-25T10:30:00Z"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid order data
- `404 Not Found` - Product not found

---

### 2. Get Order by ID

Retrieve a specific order.

```http
GET /orders/{id}
```

**Path Parameters:**
- `id` (int) - Order ID

**Response (200 OK):**

```json
{
  "id": 1,
  "orderId": "ORD-20260225-001",
  "userId": "user123",
  "cartTotal": 1299.99,
  "discountAmount": 100,
  "finalAmount": 1199.99,
  "status": "Pending",
  "items": [
    {
      "id": 1,
      "orderId": 1,
      "productId": 1,
      "productName": "Dell XPS 13 Laptop",
      "price": 1299.99,
      "quantity": 1
    }
  ],
  "createdAt": "2026-02-25T10:30:00Z"
}
```

**Error Responses:**
- `404 Not Found` - Order doesn't exist

---

### 3. Get Orders by User ID

Retrieve all orders for a specific user.

```http
GET /orders/user/{userId}
```

**Path Parameters:**
- `userId` (string) - User identifier

**Response (200 OK):**

```json
{
  "orders": [
    {
      "id": 1,
      "orderId": "ORD-20260225-001",
      "userId": "user123",
      "finalAmount": 1199.99,
      "status": "Pending",
      "createdAt": "2026-02-25T10:30:00Z"
    }
  ],
  "totalOrders": 1
}
```

---

### 4. Confirm Order

Confirm an order for processing.

```http
PUT /orders/{id}/confirm
Content-Type: application/json

{
  "status": "Confirmed"
}
```

**Path Parameters:**
- `id` (int) - Order ID

**Request Body:**
- `status` (string, required) - Must be "Confirmed"

**Response (200 OK):**

```json
{
  "id": 1,
  "orderId": "ORD-20260225-001",
  "status": "Confirmed",
  "confirmedAt": "2026-02-25T10:35:00Z"
}
```

**Error Responses:**
- `404 Not Found` - Order doesn't exist
- `400 Bad Request` - Order cannot be confirmed (already confirmed/cancelled)

---

### 5. Ship Order

Mark order as shipped with tracking information.

```http
PUT /orders/{id}/ship
Content-Type: application/json

{
  "status": "Shipped",
  "trackingNumber": "TRACK-1234567890"
}
```

**Path Parameters:**
- `id` (int) - Order ID

**Request Body:**
- `status` (string, required) - Must be "Shipped"
- `trackingNumber` (string, required) - Shipping tracking number

**Response (200 OK):**

```json
{
  "id": 1,
  "orderId": "ORD-20260225-001",
  "status": "Shipped",
  "trackingNumber": "TRACK-1234567890",
  "shippedAt": "2026-02-25T14:00:00Z"
}
```

---

### 6. Cancel Order

Cancel an existing order.

```http
DELETE /orders/{id}
```

**Path Parameters:**
- `id` (int) - Order ID

**Response (200 OK or 204 No Content):**

```json
{
  "id": 1,
  "orderId": "ORD-20260225-001",
  "status": "Cancelled",
  "cancelledAt": "2026-02-25T11:00:00Z"
}
```

**Error Responses:**
- `404 Not Found` - Order doesn't exist
- `400 Bad Request` - Order cannot be cancelled

---

## Email Service API

**Base URL:** `http://localhost:5005/api`

### 1. Get Email Log by ID

Retrieve an email sending log.

```http
GET /email/{emailId}
```

**Path Parameters:**
- `emailId` (int) - Email log ID

**Response (200 OK):**

```json
{
  "id": 1,
  "recipient": "user@example.com",
  "subject": "Order Confirmation - ORD-20260225-001",
  "status": "Sent",
  "sentAt": "2026-02-25T10:30:30Z"
}
```

---

### 2. Get User's Emails

Retrieve all emails sent to a user.

```http
GET /email/user/{userId}
```

**Path Parameters:**
- `userId` (string) - User identifier

**Response (200 OK):**

```json
{
  "emails": [
    {
      "id": 1,
      "recipient": "user@example.com",
      "subject": "Order Confirmation - ORD-20260225-001",
      "status": "Sent",
      "sentAt": "2026-02-25T10:30:30Z"
    },
    {
      "id": 2,
      "recipient": "user@example.com",
      "subject": "Your Order has Shipped - TRACK-1234567890",
      "status": "Sent",
      "sentAt": "2026-02-25T14:00:30Z"
    }
  ]
}
```

---

## Reward Service API

**Base URL:** `http://localhost:5006/api`

### 1. Get User Reward Points

Retrieve current reward points for a user.

```http
GET /reward/user/{userId}/points
```

**Path Parameters:**
- `userId` (string) - User identifier

**Response (200 OK):**

```json
{
  "userId": "user123",
  "currentBalance": 5264.97,
  "totalEarned": 5264.97,
  "totalUsed": 0,
  "transactions": [
    {
      "id": 1,
      "transactionType": "OrderCreated",
      "pointsEarned": 5264.97,
      "orderId": "ORD-20260225-001",
      "transactionDate": "2026-02-25T10:30:00Z"
    }
  ]
}
```

---

### 2. Get User Reward Tier

Retrieve user's reward tier information.

```http
GET /reward/user/{userId}/tier
```

**Path Parameters:**
- `userId` (string) - User identifier

**Response (200 OK):**

```json
{
  "userId": "user123",
  "tier": "Silver",
  "minimumPoints": 1000,
  "currentPoints": 5264.97,
  "bonusMultiplier": 1.5,
  "benefits": "1.5x points on all purchases, exclusive offers"
}
```

---

### 3. Redeem Reward Points

Redeem reward points for discount.

```http
POST /reward/user/{userId}/redeem
Content-Type: application/json

{
  "pointsToRedeem": 1000
}
```

**Path Parameters:**
- `userId` (string) - User identifier

**Request Body:**
- `pointsToRedeem` (decimal, required) - Points to redeem

**Response (200 OK):**

```json
{
  "userId": "user123",
  "pointsRedeemed": 1000,
  "discountAmount": 10.00,
  "newBalance": 4264.97,
  "redeemDate": "2026-02-25T15:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request` - Insufficient points
- `404 Not Found` - User not found

---

## HTTP Status Codes

| Code | Meaning | Use Case |
|------|---------|----------|
| 200 | OK | Request successful |
| 201 | Created | Resource created |
| 204 | No Content | Successful deletion |
| 400 | Bad Request | Invalid input data |
| 401 | Unauthorized | Authentication required |
| 403 | Forbidden | Access denied |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Server error |
| 503 | Service Unavailable | Service down |

---

## Authentication (Future)

Currently not implemented. Phase 4 will add:
- JWT Bearer token authentication
- Role-based access control (RBAC)
- API key authentication for services

---

## Rate Limiting (Future)

Phase 4 will implement:
- 100 requests per minute per user
- 1000 requests per minute per IP
- Returned in response headers: `X-RateLimit-Limit`, `X-RateLimit-Remaining`

---

## Pagination Standard

All paginated endpoints follow this format:

```
?pageNumber=1&pageSize=10
```

**Response includes:**
- `pageNumber` - Current page
- `pageSize` - Items per page
- `totalItems` - Total item count
- `totalPages` - Total page count
- `items` - Array of items

---

## Error Handling

All errors follow standard format:

```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "quantity",
      "message": "Quantity must be greater than 0"
    }
  ],
  "timestamp": "2026-02-25T10:30:00Z"
}
```

---

## Testing with cURL

### Get Products
```bash
curl -X GET "http://localhost:5001/api/products?pageNumber=1&pageSize=10"
```

### Add to Cart
```bash
curl -X POST "http://localhost:5002/api/cart/user123/items" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "productName": "Dell XPS 13 Laptop",
    "price": 1299.99,
    "quantity": 1
  }'
```

### Validate Coupon
```bash
curl -X GET "http://localhost:5004/api/coupon/validate?code=SAVE10&cartTotal=1299.99"
```

### Create Order
```bash
curl -X POST "http://localhost:5003/api/orders" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user123",
    "cartTotal": 1299.99,
    "discountAmount": 129.99,
    "finalAmount": 1169.99,
    "couponCode": "SAVE10",
    "items": [{"productId": 1, "productName": "Dell XPS 13 Laptop", "price": 1299.99, "quantity": 1}]
  }'
```

---

## Postman Collection

Import `Mango-eCommerce-Phase3.postman_collection.json` for pre-configured requests.

---

*Last Updated: February 25, 2026*
*For detailed workflow examples, see COMPLETE_WORKFLOW.md*
