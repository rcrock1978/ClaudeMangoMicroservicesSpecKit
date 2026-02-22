# Reward API Contract

**Service**: Mango.Services.RewardAPI
**Base Path**: `/api/v1/rewards`
**Authentication**: All endpoints require JWT Bearer token.

This service is primarily event-driven. It consumes OrderConfirmed
events to credit loyalty points.

---

## Message Consumers (Event-Driven)

### OrderConfirmedConsumer

**Listens to**: `OrderConfirmedEvent`
**Action**: Calculate points (1 point per $1 before discounts,
rounded to nearest whole number) and credit to user's balance.
Create RewardsActivity record for audit.

---

## GET /api/v1/rewards

Get current user's rewards balance and activity history.

**Response 200**:
```json
{
  "isSuccess": true,
  "result": {
    "userId": "string",
    "totalPoints": 150,
    "activities": [
      {
        "rewardsActivityId": 1,
        "orderId": 42,
        "pointsEarned": 50,
        "orderTotal": 50.00,
        "activityDate": "2026-02-23T10:30:00Z"
      }
    ]
  }
}
```

---

## GET /api/v1/rewards/balance

Get only the current user's points balance (lightweight).

**Response 200**:
```json
{
  "isSuccess": true,
  "result": {
    "totalPoints": 150
  }
}
```
