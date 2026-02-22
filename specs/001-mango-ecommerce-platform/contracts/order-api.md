# Order API Contract

**Service**: Mango.Services.OrderAPI
**Base Path**: `/api/v1/orders`
**Authentication**: All endpoints require JWT Bearer token.

---

## POST /api/v1/orders/create

Create an order from checkout data. Called internally by cart service
after successful payment.

**Request Body**:
```json
{
  "userId": "string",
  "name": "string",
  "email": "string",
  "phone": "string (optional)",
  "couponCode": "string (optional)",
  "discount": 5.00,
  "orderTotal": 45.00,
  "orderDetails": [
    {
      "productId": 1,
      "productName": "Mango Lassi",
      "price": 25.00,
      "count": 2
    }
  ],
  "stripeSessionId": "string (optional)",
  "paymentIntentId": "string (optional)"
}
```

**Response 201**:
```json
{
  "isSuccess": true,
  "result": {
    "orderHeaderId": 1,
    "status": "Pending"
  }
}
```

---

## GET /api/v1/orders

Get current user's order history (customers) or all orders (admins).

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| status | string? | null | Filter by status |
| page | int | 1 | Page number |
| pageSize | int | 10 | Items per page |

**Response 200**: Paginated list of orders with header info

---

## GET /api/v1/orders/{id}

Get full order details.

**Response 200**:
```json
{
  "isSuccess": true,
  "result": {
    "orderHeaderId": 1,
    "userId": "string",
    "name": "string",
    "email": "string",
    "couponCode": "SAVE10",
    "discount": 5.00,
    "orderTotal": 45.00,
    "status": "Confirmed",
    "orderTime": "2026-02-23T10:30:00Z",
    "paymentIntentId": "pi_xxx",
    "orderDetails": [
      {
        "productId": 1,
        "productName": "Mango Lassi",
        "price": 25.00,
        "count": 2
      }
    ]
  }
}
```

**Response 404**: Order not found
**Response 403**: Not owner and not admin

---

## POST /api/v1/orders/{id}/update-status

Update order status. **Requires: Administrator role** (except cancel).

**Request Body**:
```json
{
  "newStatus": "Confirmed"
}
```

**Valid transitions**: Pending→Confirmed→Processing→Shipped→Delivered
**Cancel**: Allowed from Pending or Confirmed by customer or admin.

**Response 200**: Status updated
**Response 400**: Invalid transition
**Response 404**: Order not found

---

## POST /api/v1/orders/{id}/cancel

Cancel an order. Customer can cancel own orders; admin can cancel any.

**Response 200**: Order cancelled
**Response 400**: Order not in cancellable status
**Response 403**: Not owner and not admin
**Response 404**: Order not found

---

## POST /api/v1/orders/validate-stripe

Webhook callback from Stripe to confirm payment.

**Request Body**: Stripe webhook payload
**Response 200**: Payment validated, order status → Confirmed
**Response 400**: Invalid payload
