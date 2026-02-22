# Auth API Contract

**Service**: Mango.Services.AuthAPI
**Base Path**: `/api/v1/auth`

---

## POST /api/v1/auth/register

Register a new user account.

**Request Body**:
```json
{
  "name": "string (required, max 100)",
  "email": "string (required, valid email, max 256)",
  "password": "string (required, min 8, 1 upper, 1 number, 1 special)",
  "phoneNumber": "string (optional, max 20)"
}
```

**Response 200**:
```json
{
  "isSuccess": true,
  "message": "Registration successful"
}
```

**Response 400**: Validation errors (password complexity, email format)
**Response 409**: Registration failed (generic message, no email leak)

---

## POST /api/v1/auth/login

Authenticate user and issue JWT.

**Request Body**:
```json
{
  "email": "string (required)",
  "password": "string (required)"
}
```

**Response 200**:
```json
{
  "isSuccess": true,
  "result": {
    "accessToken": "string (JWT)",
    "refreshToken": "string",
    "expiresIn": 1800,
    "user": {
      "id": "string",
      "name": "string",
      "email": "string",
      "roles": ["Customer"]
    }
  }
}
```

**Response 401**: Invalid credentials (generic message)
**Response 423**: Account locked (generic message, no email leak)

---

## POST /api/v1/auth/refresh

Refresh an expired access token.

**Request Body**:
```json
{
  "accessToken": "string (expired JWT)",
  "refreshToken": "string"
}
```

**Response 200**: Same as login response with new tokens
**Response 401**: Invalid or expired refresh token

---

## POST /api/v1/auth/assign-role

Assign a role to a user. **Requires: Administrator role.**

**Request Body**:
```json
{
  "email": "string (required)",
  "roleName": "string (required)"
}
```

**Response 200**: Role assigned successfully
**Response 404**: User not found
**Response 403**: Insufficient permissions
