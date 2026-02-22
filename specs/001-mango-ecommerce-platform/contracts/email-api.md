# Email API Contract

**Service**: Mango.Services.EmailAPI
**Base Path**: `/api/v1/email`

This service is primarily event-driven. It consumes messages from
RabbitMQ and sends transactional emails. The HTTP endpoints are
for health monitoring and admin log access only.

---

## Message Consumers (Event-Driven)

### UserRegisteredConsumer

**Listens to**: `UserRegisteredEvent`
**Action**: Send welcome email to new user
**Template**: Welcome

### OrderConfirmedConsumer

**Listens to**: `OrderConfirmedEvent`
**Action**: Send order confirmation email with order details
**Template**: OrderConfirmation

---

## GET /api/v1/email/logs

View email send history. **Requires: Administrator role.**

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| status | string? | null | Filter: "Queued", "Sent", "Failed" |
| templateType | string? | null | Filter by template |
| page | int | 1 | Page number |
| pageSize | int | 20 | Items per page |

**Response 200**: Paginated list of email logs

---

## POST /api/v1/email/retry/{emailLogId}

Retry a failed email. **Requires: Administrator role.**

**Response 200**: Email re-queued
**Response 404**: Email log not found
**Response 400**: Email is not in Failed status
