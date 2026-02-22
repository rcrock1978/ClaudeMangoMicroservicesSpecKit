# Quickstart: Mango Microservices E-Commerce Platform

**Branch**: `001-mango-ecommerce-platform`
**Date**: 2026-02-23

## Prerequisites

- .NET 10 SDK
- Docker Desktop (with Docker Compose)
- SQL Server 2022+ (or use Docker container)
- Git

## 1. Clone and Checkout

```bash
git clone <repository-url>
cd mango-microservices
git checkout 001-mango-ecommerce-platform
```

## 2. Start Infrastructure (Docker Compose)

```bash
docker-compose -f docker/docker-compose.yml up -d sqlserver rabbitmq redis
```

This starts:
- **SQL Server** on port 1433
- **RabbitMQ** on port 5672 (management UI: 15672)
- **Redis** on port 6379

Wait ~30 seconds for SQL Server to initialize.

## 3. Apply Database Migrations

Each service has its own database. Run migrations per service:

```bash
# From repository root
dotnet ef database update --project src/Mango.Services.AuthAPI/Mango.Services.Auth.Infrastructure
dotnet ef database update --project src/Mango.Services.ProductAPI/Mango.Services.Product.Infrastructure
dotnet ef database update --project src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.Infrastructure
dotnet ef database update --project src/Mango.Services.CouponAPI/Mango.Services.Coupon.Infrastructure
dotnet ef database update --project src/Mango.Services.OrderAPI/Mango.Services.Order.Infrastructure
dotnet ef database update --project src/Mango.Services.EmailAPI/Mango.Services.Email.Infrastructure
dotnet ef database update --project src/Mango.Services.RewardAPI/Mango.Services.Reward.Infrastructure
```

## 4. Run All Services

Option A — Docker Compose (all at once):

```bash
docker-compose -f docker/docker-compose.yml up -d
```

Option B — Individually for debugging:

```bash
# Terminal 1: Auth API (port 7001)
dotnet run --project src/Mango.Services.AuthAPI/Mango.Services.Auth.API

# Terminal 2: Product API (port 7002)
dotnet run --project src/Mango.Services.ProductAPI/Mango.Services.Product.API

# Terminal 3: Coupon API (port 7003)
dotnet run --project src/Mango.Services.CouponAPI/Mango.Services.Coupon.API

# Terminal 4: Cart API (port 7004)
dotnet run --project src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.API

# Terminal 5: Order API (port 7005)
dotnet run --project src/Mango.Services.OrderAPI/Mango.Services.Order.API

# Terminal 6: Email API (port 7006)
dotnet run --project src/Mango.Services.EmailAPI/Mango.Services.Email.API

# Terminal 7: Reward API (port 7007)
dotnet run --project src/Mango.Services.RewardAPI/Mango.Services.Reward.API

# Terminal 8: Gateway (port 7100)
dotnet run --project src/Mango.GatewaySolution

# Terminal 9: Web Frontend (port 7200)
dotnet run --project src/Mango.Web
```

## 5. Verify Health

Each service exposes health endpoints:

```bash
# Check all services
curl http://localhost:7001/health/ready  # Auth
curl http://localhost:7002/health/ready  # Product
curl http://localhost:7003/health/ready  # Coupon
curl http://localhost:7004/health/ready  # Cart
curl http://localhost:7005/health/ready  # Order
curl http://localhost:7006/health/ready  # Email
curl http://localhost:7007/health/ready  # Reward
curl http://localhost:7100/health/ready  # Gateway
```

## 6. Swagger Documentation

Each API service exposes Swagger UI:

- Auth: http://localhost:7001/swagger
- Product: http://localhost:7002/swagger
- Coupon: http://localhost:7003/swagger
- Cart: http://localhost:7004/swagger
- Order: http://localhost:7005/swagger
- Reward: http://localhost:7007/swagger

## 7. Smoke Test — End-to-End Purchase

1. **Register**: POST to `/api/v1/auth/register` with name, email, password
2. **Login**: POST to `/api/v1/auth/login` → get JWT token
3. **Browse products**: GET `/api/v1/products`
4. **Add to cart**: POST `/api/v1/cart/upsert` with product and quantity
5. **Apply coupon**: POST `/api/v1/cart/apply-coupon`
6. **Checkout**: POST `/api/v1/cart/checkout` → get Stripe session
7. **Verify order**: GET `/api/v1/orders` → order appears with status
8. **Check rewards**: GET `/api/v1/rewards/balance` → points credited

## 8. Run Tests

```bash
# All unit tests
dotnet test src/Mango.Microservices.sln --filter "Category=Unit"

# All integration tests (requires Docker for Testcontainers)
dotnet test src/Mango.Microservices.sln --filter "Category=Integration"

# All contract tests
dotnet test src/Mango.Microservices.sln --filter "Category=Contract"

# Everything
dotnet test src/Mango.Microservices.sln
```

## Service Port Map

| Service | Port | Swagger |
|---------|------|---------|
| Auth API | 7001 | /swagger |
| Product API | 7002 | /swagger |
| Coupon API | 7003 | /swagger |
| Cart API | 7004 | /swagger |
| Order API | 7005 | /swagger |
| Email API | 7006 | N/A |
| Reward API | 7007 | /swagger |
| Gateway | 7100 | N/A |
| Web (MVC) | 7200 | N/A |
| SQL Server | 1433 | — |
| RabbitMQ | 5672 / 15672 | Management UI |
| Redis | 6379 | — |

## Environment Variables

Key environment variables (set in appsettings or docker-compose):

| Variable | Description | Default (dev) |
|----------|-------------|---------------|
| ConnectionStrings__DefaultConnection | SQL Server connection | Per-service DB |
| JwtOptions__Secret | JWT signing key | Dev secret |
| JwtOptions__Issuer | JWT issuer | mango-auth |
| JwtOptions__Audience | JWT audience | mango-client |
| RabbitMq__Host | RabbitMQ hostname | localhost |
| RabbitMq__Username | RabbitMQ user | guest |
| RabbitMq__Password | RabbitMQ password | guest |
| Redis__Connection | Redis connection string | localhost:6379 |
| Stripe__SecretKey | Stripe API secret key | Test key |
| Stripe__PublishableKey | Stripe public key | Test key |
