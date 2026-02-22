# Implementation Plan: Mango Microservices E-Commerce Platform

**Branch**: `001-mango-ecommerce-platform` | **Date**: 2026-02-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-mango-ecommerce-platform/spec.md`

## Summary

Build a full-stack e-commerce platform composed of 9 microservices plus
a shared messaging library, following Clean Architecture with DDD per
service. The platform covers the complete e-commerce lifecycle:
authentication, product catalog, shopping cart, coupons, order
processing, email notifications, loyalty rewards, an API gateway, and
a server-rendered MVC frontend. All services communicate asynchronously
via RabbitMQ (MassTransit), persist data in dedicated SQL Server
databases via EF Core 10, and are containerized with Docker for local
development and Kubernetes for production deployment.

## Technical Context

**Language/Version**: C# 13 / .NET 10.x
**Primary Dependencies**: ASP.NET Core 10 (MVC + Web API), Entity Framework Core 10, MassTransit (RabbitMQ transport), Ocelot API Gateway, AutoMapper, FluentValidation, Serilog, OpenTelemetry .NET SDK, Swashbuckle (Swagger/OpenAPI), Microsoft.AspNetCore.Authentication.JwtBearer
**Storage**: SQL Server 2022+ (one database per microservice), Redis 7.x+ (distributed caching)
**Testing**: xUnit, Moq, Testcontainers (.NET), FluentAssertions
**Target Platform**: Linux containers (Docker/Kubernetes), cross-platform .NET 10
**Project Type**: Distributed microservices web platform (9 services + 1 MVC frontend + 1 gateway + 1 shared library)
**Performance Goals**: <200ms p95 API response, <100ms p95 DB queries, 500+ concurrent users
**Constraints**: <200ms p95 latency, database-per-service isolation, eventual consistency default, zero-downtime deployments
**Scale/Scope**: 500 concurrent users, 9 microservices, 9 SQL Server databases, ~12 deployable units

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Clean Architecture with DDD | ✅ PASS | Each service uses Domain/Application/Infrastructure/Presentation layers. Domain has zero external deps. |
| II | SOLID Principles | ✅ PASS | DI via .NET container, interface segregation per layer, repository pattern for data access. |
| III | Microservices & Bounded Contexts | ✅ PASS | 9 services = 9 bounded contexts with dedicated databases. No shared DBs. |
| IV | Event-Driven Communication (RabbitMQ) | ✅ PASS | MassTransit with RabbitMQ transport. Pub/sub, idempotent consumers, dead-letter queues, outbox pattern. |
| V | Comprehensive Testing (NON-NEGOTIABLE) | ✅ PASS | xUnit + Moq for unit, Testcontainers for integration, contract tests for APIs and messages. |
| VI | Security-First Design | ✅ PASS | JWT Bearer auth, RBAC, FluentValidation, password policy (8+ chars, complexity), account lockout. |
| VII | Performance Optimization | ✅ PASS | Async/await throughout, Redis caching, pagination on all lists, EF Core AsNoTracking, HttpClientFactory. |
| VIII | Containerized Deployment | ✅ PASS | Multi-stage Dockerfiles, docker-compose for local dev, K8s manifests for production. |
| IX | EF Core & SQL Server | ✅ PASS | EF Core 10 code-first migrations, per-service DbContext, repository/UoW pattern, soft deletes, audit columns. |
| X | Observability & Code Quality | ✅ PASS | Serilog structured logging, OpenTelemetry tracing/metrics, health checks, URL-based API versioning, .editorconfig. |

**Gate Result**: ALL PASS — proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/001-mango-ecommerce-platform/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── auth-api.md
│   ├── product-api.md
│   ├── cart-api.md
│   ├── coupon-api.md
│   ├── order-api.md
│   ├── email-api.md
│   ├── reward-api.md
│   └── message-bus-events.md
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Mango.Microservices.sln
│
├── Mango.Web/                              # ASP.NET Core MVC Frontend
│   ├── Controllers/
│   ├── Views/
│   ├── Models/
│   ├── Services/                           # HTTP clients to gateway
│   ├── wwwroot/
│   ├── Program.cs
│   └── appsettings.json
│
├── Mango.GatewaySolution/                  # Ocelot API Gateway
│   ├── Program.cs
│   ├── ocelot.json
│   └── appsettings.json
│
├── Mango.MessageBus/                       # Shared messaging library
│   ├── Events/                             # Integration event definitions
│   ├── Contracts/                          # Message interfaces
│   └── Extensions/                         # MassTransit registration helpers
│
├── Mango.Services.AuthAPI/
│   ├── Mango.Services.Auth.Domain/
│   │   ├── Entities/
│   │   └── ValueObjects/
│   ├── Mango.Services.Auth.Application/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   └── Validators/
│   ├── Mango.Services.Auth.Infrastructure/
│   │   ├── Data/                           # DbContext, Migrations
│   │   ├── Repositories/
│   │   └── Services/                       # JwtTokenGenerator, etc.
│   └── Mango.Services.Auth.API/
│       ├── Controllers/
│       ├── Program.cs
│       └── appsettings.json
│
├── Mango.Services.ProductAPI/
│   ├── Mango.Services.Product.Domain/
│   ├── Mango.Services.Product.Application/
│   ├── Mango.Services.Product.Infrastructure/
│   └── Mango.Services.Product.API/
│
├── Mango.Services.ShoppingCartAPI/
│   ├── Mango.Services.ShoppingCart.Domain/
│   ├── Mango.Services.ShoppingCart.Application/
│   ├── Mango.Services.ShoppingCart.Infrastructure/
│   └── Mango.Services.ShoppingCart.API/
│
├── Mango.Services.CouponAPI/
│   ├── Mango.Services.Coupon.Domain/
│   ├── Mango.Services.Coupon.Application/
│   ├── Mango.Services.Coupon.Infrastructure/
│   └── Mango.Services.Coupon.API/
│
├── Mango.Services.OrderAPI/
│   ├── Mango.Services.Order.Domain/
│   ├── Mango.Services.Order.Application/
│   ├── Mango.Services.Order.Infrastructure/
│   └── Mango.Services.Order.API/
│
├── Mango.Services.EmailAPI/
│   ├── Mango.Services.Email.Domain/
│   ├── Mango.Services.Email.Application/
│   ├── Mango.Services.Email.Infrastructure/
│   └── Mango.Services.Email.API/
│
├── Mango.Services.RewardAPI/
│   ├── Mango.Services.Reward.Domain/
│   ├── Mango.Services.Reward.Application/
│   ├── Mango.Services.Reward.Infrastructure/
│   └── Mango.Services.Reward.API/
│
├── tests/
│   ├── Mango.Services.Auth.UnitTests/
│   ├── Mango.Services.Auth.IntegrationTests/
│   ├── Mango.Services.Product.UnitTests/
│   ├── Mango.Services.Product.IntegrationTests/
│   ├── Mango.Services.ShoppingCart.UnitTests/
│   ├── Mango.Services.ShoppingCart.IntegrationTests/
│   ├── Mango.Services.Coupon.UnitTests/
│   ├── Mango.Services.Coupon.IntegrationTests/
│   ├── Mango.Services.Order.UnitTests/
│   ├── Mango.Services.Order.IntegrationTests/
│   ├── Mango.Services.Email.UnitTests/
│   ├── Mango.Services.Email.IntegrationTests/
│   ├── Mango.Services.Reward.UnitTests/
│   ├── Mango.Services.Reward.IntegrationTests/
│   └── Mango.ContractTests/               # Cross-service contract tests
│
├── docker/
│   ├── docker-compose.yml                  # All services + infra
│   ├── docker-compose.override.yml         # Dev overrides
│   └── Dockerfile.*                        # Per-service Dockerfiles
│
└── k8s/
    ├── namespace.yaml
    ├── auth/
    ├── product/
    ├── cart/
    ├── coupon/
    ├── order/
    ├── email/
    ├── reward/
    ├── gateway/
    ├── web/
    └── infrastructure/                     # SQL Server, RabbitMQ, Redis
```

**Structure Decision**: Microservices monorepo with per-service Clean
Architecture (4 layers each). Shared messaging contracts live in
`Mango.MessageBus`. Tests are co-located by service in `tests/`.
Infrastructure configs (Docker, K8s) are at root level.

## Complexity Tracking

> No constitution violations detected. All principles satisfied by design.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | — | — |
