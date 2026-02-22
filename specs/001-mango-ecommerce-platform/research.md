# Research: Mango Microservices E-Commerce Platform

**Branch**: `001-mango-ecommerce-platform`
**Date**: 2026-02-23
**Status**: Complete — all decisions resolved, no NEEDS CLARIFICATION remaining.

## R1: Message Bus Implementation — MassTransit vs Raw RabbitMQ Client

**Decision**: MassTransit with RabbitMQ transport

**Rationale**: MassTransit provides a mature abstraction over RabbitMQ
that includes built-in support for saga orchestration, outbox pattern,
retry policies with exponential backoff, dead-letter queues, and
message scheduling. It eliminates boilerplate for exchange/queue
topology management and provides strongly-typed consumers that align
with Clean Architecture dependency inversion.

**Alternatives considered**:
- Raw RabbitMQ.Client: Maximum control but requires manual
  implementation of retry, dead-letter, serialization, and topology
  management. Significantly more code and maintenance burden.
- NServiceBus: Feature-rich but commercially licensed. MassTransit
  is MIT-licensed and community-driven.
- CAP (DotNetCore.CAP): Lighter-weight outbox pattern library but
  less mature saga and retry support compared to MassTransit.

## R2: API Gateway — Ocelot vs YARP

**Decision**: Ocelot

**Rationale**: Ocelot is specifically designed as an API gateway for
.NET microservices with declarative JSON-based routing configuration.
It provides built-in support for authentication forwarding, rate
limiting, load balancing, and request aggregation. The user explicitly
specified Ocelot in requirements.

**Alternatives considered**:
- YARP (Yet Another Reverse Proxy): Higher performance and more
  flexible programmatic configuration but more complex setup for
  standard gateway patterns. Better suited for custom proxy scenarios.
- Kong/Envoy: Language-agnostic gateways with rich plugin ecosystems
  but add operational complexity and are external to .NET ecosystem.

## R3: Payment Integration Strategy

**Decision**: Stripe via a provider-agnostic abstraction layer

**Rationale**: Stripe is the most widely adopted payment processor
with excellent .NET SDK support, comprehensive documentation, and
a test mode for development. The implementation will use an
`IPaymentService` interface in the Application layer with a Stripe
implementation in Infrastructure, allowing future provider swaps.

**Alternatives considered**:
- PayPal: Popular but more complex integration. Can be added later
  behind the same abstraction.
- Braintree: Owned by PayPal, good .NET support but smaller market
  share than Stripe.
- Mock-only for v1: Considered but rejected — payment integration
  is a core acceptance criterion (FR-022, User Story 1).

## R4: Clean Architecture Layer Communication Pattern

**Decision**: CQRS-lite with MediatR for application layer orchestration

**Rationale**: MediatR enables clean separation of commands and queries
within the Application layer without introducing full event sourcing
complexity. Each use case becomes a discrete handler class (single
responsibility), and the pipeline supports cross-cutting concerns
(validation, logging) via behaviors. This aligns with SOLID principles
and keeps controllers thin.

**Alternatives considered**:
- Direct service injection: Simpler but leads to fat service classes
  that violate SRP. Controllers become coupled to specific service
  implementations.
- Full CQRS with separate read/write stores: Over-engineered for v1.
  Can be introduced later for high-read services (Product Catalog)
  if performance demands it.

## R5: Authentication Token Strategy

**Decision**: Short-lived JWT access tokens (30 min) + longer-lived
refresh tokens (7 days) stored server-side

**Rationale**: Short-lived access tokens minimize the window of
compromise. Refresh tokens stored in the Auth database enable
server-side revocation. The MVC frontend stores the access token
in an HTTP-only secure cookie for CSRF protection, and includes
the refresh token flow transparently.

**Alternatives considered**:
- Long-lived JWT only: Simpler but no revocation capability and
  longer exposure window if compromised.
- Session-based auth: Doesn't align with microservices architecture
  where multiple services need to validate authentication independently.
- OAuth2/OpenID Connect with IdentityServer: Over-engineered for a
  single-platform system. Can be introduced later if third-party
  integrations require it.

## R6: Database Migration Strategy

**Decision**: EF Core code-first migrations with per-service migration
projects, applied at startup in development and via CI/CD pipeline
in production

**Rationale**: Code-first migrations keep the schema in sync with
domain entities and are version-controlled. Each service manages its
own migrations independently, supporting the database-per-service
pattern. Development environments auto-migrate on startup for rapid
iteration; production environments run migrations as a separate
deployment step for safety.

**Alternatives considered**:
- Database-first: Conflicts with DDD approach where the domain model
  drives persistence, not the other way around.
- DbUp or FluentMigrator: Additional tooling when EF Core migrations
  are sufficient and tightly integrated.
- SQL scripts: Manual, error-prone, and not tied to the entity model.

## R7: Frontend Architecture — MVC vs SPA

**Decision**: ASP.NET Core MVC (server-rendered) with targeted
JavaScript for interactivity

**Rationale**: The spec explicitly states server-rendered MVC frontend.
Server-side rendering provides better SEO for product pages, simpler
deployment (single dotnet project), and eliminates the need for a
separate frontend build pipeline. JavaScript is used only for dynamic
features (cart updates, search-as-you-type).

**Alternatives considered**:
- Blazor Server: .NET-native but requires persistent WebSocket
  connections which adds scaling complexity.
- React/Angular SPA: Richer interactivity but adds build complexity,
  CORS configuration, and a separate deployment unit.
- Blazor WASM: Client-side .NET but large initial download and
  limited browser API access.

## R8: Outbox Pattern Implementation

**Decision**: MassTransit Transactional Outbox with EF Core

**Rationale**: MassTransit 8+ provides a first-class EF Core
transactional outbox that stores published messages in the same
database transaction as the business operation. A background
delivery service processes the outbox table and publishes to
RabbitMQ, guaranteeing at-least-once delivery without custom
implementation.

**Alternatives considered**:
- Custom outbox table + background worker: More control but
  duplicates functionality already provided by MassTransit.
- Polling publisher: Simpler but higher latency for message delivery.
- CDC (Change Data Capture): Over-engineered for this scale.

## R9: Distributed Caching Strategy

**Decision**: Redis via StackExchange.Redis with IDistributedCache
abstraction

**Rationale**: Redis provides sub-millisecond reads for cached data.
Using ASP.NET Core's IDistributedCache interface keeps the caching
layer swappable. Initial caching targets: product catalog (5-min TTL),
coupon validation results (1-min TTL). Cache invalidation is
event-driven via MassTransit consumers listening for ProductUpdated
and CouponUpdated events.

**Alternatives considered**:
- In-memory cache only: Doesn't work across multiple service instances
  in Kubernetes.
- NCache: Commercial license, overkill for this scale.
- Memcached: Lacks Redis data structures (sorted sets for leaderboards,
  pub/sub for invalidation).

## R10: Observability Stack

**Decision**: OpenTelemetry SDK → OTLP exporter (configurable backend)

**Rationale**: OpenTelemetry provides vendor-neutral instrumentation.
Traces are collected for HTTP requests (ASP.NET Core), database queries
(EF Core), and message processing (MassTransit). Metrics include
request rate, error rate, and latency histograms. The OTLP exporter
sends data to any compatible backend (Jaeger for dev, Grafana/Tempo
for production). Serilog integrates with OpenTelemetry for correlated
log-trace linking.

**Alternatives considered**:
- Application Insights: Azure-specific, vendor lock-in.
- Datadog/New Relic: Commercial SaaS, adds cost.
- Custom logging only: Insufficient for distributed trace correlation
  across 9 services.
