# Product API Contract

**Service**: Mango.Services.ProductAPI
**Base Path**: `/api/v1/products`

---

## GET /api/v1/products

List products with filtering, search, sorting, and pagination.

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| categoryId | int? | null | Filter by category |
| search | string? | null | Keyword search on name and description |
| sortBy | string? | "newest" | Sort: "price-asc", "price-desc", "name", "newest" |
| page | int | 1 | Page number (1-based) |
| pageSize | int | 10 | Items per page (max 50) |

**Response 200**:
```json
{
  "isSuccess": true,
  "result": {
    "items": [
      {
        "productId": 1,
        "name": "string",
        "description": "string",
        "price": 29.99,
        "imageUrl": "string",
        "categoryId": 1,
        "categoryName": "string",
        "isAvailable": true
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 42,
    "totalPages": 5
  }
}
```

---

## GET /api/v1/products/{id}

Get a single product by ID.

**Response 200**: Single product object (same shape as list item)
**Response 404**: Product not found

---

## POST /api/v1/products

Create a new product. **Requires: Administrator role.**

**Request Body**:
```json
{
  "name": "string (required, max 200)",
  "description": "string (required, max 1000)",
  "price": 29.99,
  "imageUrl": "string (required, max 500)",
  "categoryId": 1
}
```

**Response 201**: Created product with generated ID
**Response 400**: Validation errors
**Response 403**: Insufficient permissions

---

## PUT /api/v1/products/{id}

Update an existing product. **Requires: Administrator role.**

**Request Body**: Same as POST
**Response 200**: Updated product
**Response 404**: Product not found

---

## DELETE /api/v1/products/{id}

Delete a product (soft delete). **Requires: Administrator role.**

**Response 200**: Product deleted
**Response 404**: Product not found

---

## GET /api/v1/products/categories

List all categories.

**Response 200**:
```json
{
  "isSuccess": true,
  "result": [
    { "categoryId": 1, "name": "Appetizers" },
    { "categoryId": 2, "name": "Desserts" }
  ]
}
```
