# Message Bus Event Contracts

**Library**: Mango.MessageBus
**Transport**: RabbitMQ via MassTransit
**Serialization**: JSON (System.Text.Json)

All events use MassTransit conventions for exchange/queue naming.
All consumers MUST be idempotent (FR-033).

---

## UserRegisteredEvent

**Published by**: Auth Service (after successful registration)
**Consumed by**: Email Service (send welcome email)

```json
{
  "userId": "string (GUID)",
  "name": "string",
  "email": "string",
  "registeredAt": "2026-02-23T10:00:00Z"
}
```

---

## OrderPlacedEvent

**Published by**: Order Service (after order creation)
**Consumed by**: Email Service, Reward Service

```json
{
  "orderHeaderId": 1,
  "userId": "string (GUID)",
  "name": "string",
  "email": "string",
  "orderTotal": 50.00,
  "discount": 5.00,
  "orderTotalBeforeDiscount": 55.00,
  "couponCode": "SAVE10",
  "orderTime": "2026-02-23T10:30:00Z",
  "orderDetails": [
    {
      "productId": 1,
      "productName": "Mango Lassi",
      "price": 25.00,
      "count": 2
    }
  ]
}
```

---

## OrderConfirmedEvent

**Published by**: Order Service (after Stripe payment validation)
**Consumed by**: Reward Service (credit points), Email Service (send confirmation)

```json
{
  "orderHeaderId": 1,
  "userId": "string (GUID)",
  "name": "string",
  "email": "string",
  "orderTotal": 50.00,
  "orderTotalBeforeDiscount": 55.00,
  "orderTime": "2026-02-23T10:30:00Z",
  "orderDetails": [
    {
      "productId": 1,
      "productName": "Mango Lassi",
      "price": 25.00,
      "count": 2
    }
  ]
}
```

**Notes**: Reward service uses `orderTotalBeforeDiscount` for points
calculation (1 point per $1, rounded).

---

## OrderCancelledEvent

**Published by**: Order Service (after cancellation)
**Consumed by**: Email Service (send cancellation notification)

```json
{
  "orderHeaderId": 1,
  "userId": "string (GUID)",
  "email": "string",
  "cancelledBy": "string (GUID of canceller)",
  "cancelledAt": "2026-02-23T11:00:00Z"
}
```

---

## CartCheckoutEvent

**Published by**: Cart Service (after checkout initiated)
**Consumed by**: Order Service (create order)

```json
{
  "userId": "string (GUID)",
  "name": "string",
  "email": "string",
  "phone": "string (optional)",
  "couponCode": "string (optional)",
  "discount": 5.00,
  "cartTotal": 50.00,
  "cartDetails": [
    {
      "productId": 1,
      "productName": "Mango Lassi",
      "price": 25.00,
      "count": 2
    }
  ],
  "stripeSessionId": "string"
}
```

---

## Event Flow Diagram

```
Registration:
  Auth → UserRegisteredEvent → Email (welcome)

Purchase:
  Cart → CartCheckoutEvent → Order (create)
  Stripe webhook → Order (confirm)
  Order → OrderConfirmedEvent → Email (confirmation) + Reward (credit points)

Cancellation:
  Order → OrderCancelledEvent → Email (cancellation notice)
```

---

## Dead Letter & Retry Configuration

- **Retry**: 3 attempts with exponential backoff (5s, 15s, 45s)
- **Dead Letter**: After max retries, message goes to `_error` queue
- **Idempotency**: All consumers MUST check for duplicate processing
  using the event's primary key (e.g., OrderHeaderId, UserId)
- **Outbox**: All services publishing events MUST use MassTransit
  Transactional Outbox with EF Core for at-least-once delivery
