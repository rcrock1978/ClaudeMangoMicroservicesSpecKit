# Coupon API Contract

**Service**: Mango.Services.CouponAPI
**Base Path**: `/api/v1/coupons`

---

## GET /api/v1/coupons/{code}

Get coupon details by code. **Public** (used by cart service).

**Response 200**:
```json
{
  "isSuccess": true,
  "result": {
    "couponId": 1,
    "couponCode": "SAVE10",
    "discountType": "Percentage",
    "discountAmount": 10.00,
    "minAmount": 20.00,
    "expirationDate": "2026-12-31T23:59:59Z",
    "isActive": true
  }
}
```

**Response 404**: Coupon not found

---

## GET /api/v1/coupons

List all coupons. **Requires: Administrator role.**

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| includeInactive | bool | false | Include deactivated coupons |
| page | int | 1 | Page number |
| pageSize | int | 20 | Items per page |

**Response 200**: Paginated list of coupons

---

## POST /api/v1/coupons

Create a new coupon. **Requires: Administrator role.**

**Request Body**:
```json
{
  "couponCode": "string (required, max 50, unique)",
  "discountType": "Percentage | Fixed (required)",
  "discountAmount": 10.00,
  "minAmount": 20.00,
  "expirationDate": "2026-12-31T23:59:59Z"
}
```

**Response 201**: Created coupon
**Response 400**: Validation errors (duplicate code, invalid type)

---

## PUT /api/v1/coupons/{id}

Update a coupon. **Requires: Administrator role.**

**Request Body**: Same as POST
**Response 200**: Updated coupon
**Response 404**: Coupon not found

---

## DELETE /api/v1/coupons/{id}

Deactivate a coupon (soft delete). **Requires: Administrator role.**

**Response 200**: Coupon deactivated
**Response 404**: Coupon not found
