<!--
  ╔══════════════════════════════════════════════════════════════╗
  ║                    SYNC IMPACT REPORT                       ║
  ╠══════════════════════════════════════════════════════════════╣
  ║ Version Change: 0.0.0 → 1.0.0 (MAJOR - initial adoption)  ║
  ║                                                             ║
  ║ Modified Principles: N/A (initial version)                  ║
  ║                                                             ║
  ║ Added Sections:                                             ║
  ║  - 10 Core Principles (I through X)                         ║
  ║  - Technology Stack & Constraints                           ║
  ║  - Development Workflow & Quality Gates                     ║
  ║  - Governance                                               ║
  ║                                                             ║
  ║ Removed Sections: None                                      ║
  ║                                                             ║
  ║ Templates Requiring Updates:                                ║
  ║  - .specify/templates/plan-template.md        ✅ compatible ║
  ║    Constitution Check section already dynamic               ║
  ║  - .specify/templates/spec-template.md        ✅ compatible ║
  ║    FR/SC sections align with principle gates                 ║
  ║  - .specify/templates/tasks-template.md       ✅ compatible ║
  ║    Phase structure supports observability, security,         ║
  ║    testing, and polish tasks from principles                ║
  ║  - .specify/templates/checklist-template.md   ✅ compatible ║
  ║  - .specify/templates/agent-file-template.md  ✅ compatible ║
  ║                                                             ║
  ║ Deferred Items: None                                        ║
  ╚══════════════════════════════════════════════════════════════╝
-->

# Mango Microservices Constitution

## Core Principles

### I. Clean Architecture with Domain-Driven Design

Every microservice MUST follow Clean Architecture with explicit
layer separation: Domain (entities, value objects, domain events),
Application (use cases, DTOs, interfaces), Infrastructure
(persistence, messaging, external services), and Presentation
(API controllers, middleware).

- Domain layer MUST have zero dependencies on other layers.
- Each microservice MUST represent a single bounded context
  with clearly defined aggregate roots and domain boundaries.
- Cross-context communication MUST occur only through published
  integration events or well-defined API contracts, never through
  shared databases or direct domain model references.
- Ubiquitous language MUST be established per bounded context
  and reflected consistently in code, APIs, and documentation.

### II. SOLID Principles Enforcement

All code MUST adhere to SOLID principles without exception.

- **Single Responsibility**: Each class MUST have exactly one
  reason to change. Services, repositories, and handlers MUST
  NOT combine unrelated concerns.
- **Open/Closed**: Extension points MUST be provided through
  abstractions (interfaces, base classes). Behavior changes
  MUST NOT require modification of existing, tested code.
- **Liskov Substitution**: All implementations MUST be fully
  substitutable for their abstractions without altering program
  correctness.
- **Interface Segregation**: Interfaces MUST be granular and
  client-specific. No consumer MUST depend on methods it does
  not use.
- **Dependency Inversion**: All inter-layer dependencies MUST
  point inward toward the domain. Concrete implementations MUST
  be injected via .NET dependency injection, never instantiated
  directly.

### III. Microservices Architecture & Bounded Contexts

Each microservice MUST own its data, its deployment lifecycle,
and its bounded context exclusively.

- Services MUST NOT share databases. Each service MUST have its
  own SQL Server database or schema with independent migrations.
- Service boundaries MUST align with business capabilities
  (e.g., Product Catalog, Shopping Cart, Order Management,
  Payment Processing, Identity/Auth, Coupon/Promotions).
- Inter-service communication MUST be asynchronous by default
  using RabbitMQ for event-driven messaging. Synchronous HTTP
  calls are permitted only for query operations where eventual
  consistency is unacceptable.
- Every service MUST expose a well-defined API contract and
  MUST NOT leak internal implementation details.

### IV. Event-Driven Communication via RabbitMQ

All state-changing cross-service communication MUST use
asynchronous messaging through RabbitMQ.

- Integration events MUST be defined in a shared contracts
  library with versioned message schemas.
- Publishers MUST NOT know about subscribers (pub/sub pattern).
  Topic exchanges MUST be used for event fanout.
- Consumers MUST be idempotent: processing the same message
  twice MUST produce the same result.
- Dead-letter queues MUST be configured for every consumer
  queue to handle poison messages.
- Message retry policies MUST implement exponential backoff
  with a maximum retry count before dead-lettering.
- The Outbox Pattern MUST be used when publishing events as
  part of a database transaction to guarantee at-least-once
  delivery.

### V. Comprehensive Testing Strategy (NON-NEGOTIABLE)

Every feature MUST be covered by a multi-layer testing pyramid.

- **Unit Tests**: All domain logic, application services, and
  validators MUST have unit tests. Minimum 80% code coverage
  on domain and application layers. Use xUnit and Moq/NSubstitute.
- **Integration Tests**: Every repository, message handler, and
  external service integration MUST have integration tests using
  Testcontainers for SQL Server and RabbitMQ.
- **Contract Tests**: Every API endpoint MUST have contract tests
  validating request/response schemas. Every message
  producer/consumer pair MUST have contract tests verifying
  message schema compatibility.
- Test-Driven Development (TDD) is STRONGLY RECOMMENDED:
  write failing tests first, then implement to make them pass,
  then refactor. Red-Green-Refactor cycle.
- Tests MUST be independent, repeatable, and MUST NOT depend on
  external state or execution order.

### VI. Security-First Design

Security MUST be designed in from the start, never bolted on.

- **Authentication**: JWT Bearer tokens MUST be used for all
  API authentication. Tokens MUST be validated on every request.
  Token expiry MUST be enforced (short-lived access tokens,
  longer-lived refresh tokens).
- **Authorization**: Role-Based Access Control (RBAC) MUST be
  implemented. Every endpoint MUST declare its required roles
  or policies explicitly. The principle of least privilege MUST
  be followed.
- **Input Validation**: All external input MUST be validated
  using FluentValidation. No raw user input MUST reach domain
  logic unvalidated.
- **Secrets Management**: Connection strings, API keys, and
  credentials MUST NOT be stored in code or config files.
  Use environment variables or a secrets manager.
- **HTTPS**: All service-to-service and client-to-service
  communication MUST use TLS in non-development environments.
- **OWASP Top 10**: All services MUST be reviewed against OWASP
  Top 10 vulnerabilities before production deployment.

### VII. Performance Optimization

Every service MUST be designed for predictable, measurable
performance under load.

- **Response Time**: API endpoints MUST respond within 200ms
  at p95 under normal load. Database queries MUST complete
  within 100ms at p95.
- **Async/Await**: All I/O-bound operations MUST use async/await
  throughout the call chain. No sync-over-async or async-over-sync
  patterns.
- **Caching**: Frequently accessed, rarely changing data MUST
  use distributed caching (Redis) with explicit TTL policies.
  Cache invalidation MUST be event-driven.
- **Pagination**: All list endpoints MUST support pagination.
  No endpoint MUST return unbounded result sets.
- **Connection Pooling**: Database connections and HTTP clients
  MUST use connection pooling. HttpClientFactory MUST be used
  for all outbound HTTP calls.
- **EF Core Optimization**: Queries MUST use AsNoTracking for
  read-only operations. N+1 query patterns MUST be detected
  and eliminated. Compiled queries SHOULD be used for hot paths.

### VIII. Containerized Deployment with Docker & Kubernetes

Every microservice MUST be containerized and orchestration-ready.

- Each service MUST have a multi-stage Dockerfile optimized for
  minimal image size (use .NET Alpine SDK for build, ASP.NET
  runtime-deps for production).
- Docker Compose MUST be provided for local development with
  all dependencies (SQL Server, RabbitMQ, Redis).
- Kubernetes manifests (or Helm charts) MUST define Deployments,
  Services, ConfigMaps, Secrets, and HorizontalPodAutoscalers
  for each microservice.
- Health check probes (liveness and readiness) MUST be configured
  for every Kubernetes deployment.
- Container images MUST NOT run as root. A non-root user MUST
  be specified in the Dockerfile.
- Environment-specific configuration MUST be injected via
  environment variables or Kubernetes ConfigMaps/Secrets, never
  baked into images.

### IX. Data Access with Entity Framework Core & SQL Server

All data persistence MUST use Entity Framework Core with SQL
Server, following repository and unit-of-work patterns.

- Every bounded context MUST have its own DbContext. Shared
  DbContexts across services are PROHIBITED.
- Database schema changes MUST be managed through EF Core
  code-first migrations. Every migration MUST be reversible.
- Repository interfaces MUST be defined in the Application
  layer. Implementations MUST reside in the Infrastructure layer.
- Soft deletes MUST be preferred over hard deletes for
  business entities. Audit columns (CreatedAt, UpdatedAt,
  CreatedBy) MUST be present on all entities.
- Seed data MUST be provided for development and testing
  environments via EF Core data seeding.
- Sensitive data MUST be encrypted at rest where applicable.

### X. Observability, API Versioning & Code Quality

All services MUST be observable, versioned, and maintainable.

- **Structured Logging**: Serilog MUST be used with structured
  log output (JSON). Correlation IDs MUST propagate across
  service boundaries. Log levels MUST follow: Error (failures),
  Warning (degradation), Information (business events),
  Debug (diagnostics).
- **OpenTelemetry**: Distributed tracing MUST be implemented
  using OpenTelemetry SDK. Traces MUST cover HTTP requests,
  database queries, and message processing. Metrics MUST include
  request rate, error rate, and latency histograms.
- **Health Checks**: Every service MUST expose /health/live and
  /health/ready endpoints using ASP.NET Core Health Checks.
  Checks MUST verify database connectivity, RabbitMQ
  connectivity, and downstream service availability.
- **API Versioning**: All APIs MUST be versioned using URL path
  versioning (e.g., /api/v1/products). Breaking changes MUST
  increment the major version. The previous version MUST remain
  operational during a documented deprecation period.
- **Code Quality**: All projects MUST enable nullable reference
  types. Warnings MUST be treated as errors in CI. Consistent
  naming conventions (PascalCase for public members, camelCase
  for parameters/locals) MUST be enforced via .editorconfig.

## Technology Stack & Constraints

### Mandatory Technology Choices

| Layer | Technology | Version |
|---|---|---|
| Runtime | .NET | 10.x |
| Web Framework | ASP.NET Core Minimal APIs + Controllers | 10.x |
| ORM | Entity Framework Core | 10.x |
| Database | SQL Server | 2022+ |
| Message Broker | RabbitMQ | 3.13+ |
| Caching | Redis (StackExchange.Redis) | 7.x+ |
| Containerization | Docker | 24+ |
| Orchestration | Kubernetes | 1.28+ |
| Testing | xUnit + Testcontainers + Moq | Latest |
| Logging | Serilog | Latest |
| Observability | OpenTelemetry .NET SDK | Latest |
| Validation | FluentValidation | Latest |
| Auth | JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer) | 10.x |
| API Docs | Swagger / OpenAPI (Swashbuckle or NSwag) | Latest |
| API Gateway | Ocelot or YARP | Latest |

### Project Naming Conventions

- Solution: `Mango.Microservices.sln`
- Service projects: `Mango.Services.<BoundedContext>.API`
- Domain libraries: `Mango.Services.<BoundedContext>.Domain`
- Application libraries: `Mango.Services.<BoundedContext>.Application`
- Infrastructure libraries: `Mango.Services.<BoundedContext>.Infrastructure`
- Shared contracts: `Mango.MessageBus`
- Shared DTOs: `Mango.Services.<BoundedContext>.Models`
- Gateway: `Mango.Gateway`
- Integration tests: `Mango.Services.<BoundedContext>.IntegrationTests`

### Non-Functional Requirements

- **Availability**: Each service MUST target 99.9% uptime.
- **Scalability**: Services MUST scale horizontally. No
  service MUST depend on in-memory state across requests.
- **Data Consistency**: Eventual consistency is the default.
  Strong consistency is permitted only within a single
  bounded context's database transactions.
- **Deployment**: Zero-downtime deployments MUST be supported
  using rolling update strategies.

## Development Workflow & Quality Gates

### Branch Strategy

- `main` branch is the production-ready branch. Direct pushes
  are PROHIBITED.
- Feature branches MUST follow the naming convention:
  `feature/<ticket-id>-<short-description>`.
- All changes MUST go through pull requests with at least
  one approval.

### CI/CD Quality Gates

Every pull request MUST pass ALL of the following gates
before merge:

1. **Build**: Solution MUST compile with zero errors and
   zero warnings (TreatWarningsAsErrors=true).
2. **Unit Tests**: All unit tests MUST pass. Coverage MUST
   meet the 80% threshold on domain/application layers.
3. **Integration Tests**: All integration tests MUST pass.
4. **Contract Tests**: All contract tests MUST pass.
5. **Static Analysis**: Code MUST pass configured analyzers
   with no new warnings.
6. **Security Scan**: Dependency vulnerability scan MUST
   report no critical/high CVEs.
7. **Docker Build**: Container image MUST build successfully.

### Code Review Standards

- Reviewers MUST verify adherence to constitution principles.
- Every PR MUST include: description of changes, testing
  evidence, and any constitution principle considerations.
- Architecture Decision Records (ADRs) MUST be created for
  any deviation from established patterns.

## Governance

This constitution is the supreme authority for all development
decisions in the Mango Microservices platform. In any conflict
between developer preference and constitution principles, the
constitution prevails.

### Amendment Procedure

1. Any team member MAY propose an amendment via a pull request
   modifying this file.
2. Amendments MUST include rationale, impact analysis, and a
   migration plan for existing code.
3. Breaking amendments (principle removal or redefinition)
   require team-wide review and explicit approval.
4. All amendments MUST update the version number following
   semantic versioning:
   - **MAJOR**: Principle removal, redefinition, or backward-
     incompatible governance change.
   - **MINOR**: New principle added or existing principle
     materially expanded.
   - **PATCH**: Clarification, typo fix, or non-semantic
     refinement.

### Compliance Review

- Every sprint retrospective MUST include a constitution
  compliance checkpoint.
- Architectural reviews MUST reference constitution principles
  when evaluating designs.
- New team members MUST review this constitution as part of
  onboarding.

### Runtime Guidance

For day-to-day development guidance beyond these principles,
refer to `CLAUDE.md` at the repository root (if present) and
individual service README files.

**Version**: 1.0.0 | **Ratified**: 2026-02-23 | **Last Amended**: 2026-02-23
