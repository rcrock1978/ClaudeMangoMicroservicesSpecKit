# Shopping Cart API Contract

**Service**: Mango.Services.ShoppingCartAPI
**Base Path**: `/api/v1/cart`
**Authentication**: All endpoints require JWT Bearer token.

---

## GET /api/v1/cart

Get the current user's cart.

**Response 200**:
```json
{
  "isSuccess": true,
  "result": {
    "cartHeaderId": 1,
    "userId": "string",
    "couponCode": "SAVE10",
    "discount": 5.00,
    "cartTotal": 45.00,
    "cartDetails": [
      {
        "cartDetailsId": 1,
        "productId": 1,
        "productName": "Mango Lassi",
        "price": 25.00,
        "count": 2
      }
    ]
  }
}
```

**Response 200 (empty cart)**: result with empty cartDetails array

---

## POST /api/v1/cart/upsert

Add item to cart or update quantity if already exists.

**Request Body**:
```json
{
  "productId": 1,
  "count": 2
}
```

**Response 200**: Updated cart
**Response 400**: Product unavailable or invalid quantity

---

## DELETE /api/v1/cart/remove/{cartDetailsId}

Remove a specific item from cart.

**Response 200**: Updated cart
**Response 404**: Cart item not found

---

## POST /api/v1/cart/apply-coupon

Apply a coupon code to the cart.

**Request Body**:
```json
{
  "couponCode": "SAVE10"
}
```

**Response 200**: Cart with discount applied
**Response 400**: Invalid, expired, or minimum not met

---

## POST /api/v1/cart/remove-coupon

Remove applied coupon from cart.

**Response 200**: Cart with discount removed

---

## POST /api/v1/cart/checkout

Convert cart to order. Validates all items and prices.

**Request Body**:
```json
{
  "name": "string (required)",
  "email": "string (required)",
  "phone": "string (optional)"
}
```

**Response 200**: Stripe session URL for payment
**Response 400**: Cart empty, product unavailable, or price changed
