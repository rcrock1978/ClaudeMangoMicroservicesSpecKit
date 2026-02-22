# Tasks: Mango Microservices E-Commerce Platform

**Input**: Design documents from `/specs/001-mango-ecommerce-platform/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Included — Constitution Principle V (Comprehensive Testing) is NON-NEGOTIABLE.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Service projects: `src/Mango.Services.<Context>API/Mango.Services.<Context>.<Layer>/`
- Shared library: `src/Mango.MessageBus/`
- Gateway: `src/Mango.GatewaySolution/`
- Frontend: `src/Mango.Web/`
- Tests: `src/tests/Mango.Services.<Context>.<TestType>/`

---

## Phase 1: Setup (Project Initialization)

**Purpose**: Create the solution structure, all projects, and shared infrastructure scaffolding.

- [ ] T001 Create solution file `src/Mango.Microservices.sln` and root directory structure per plan.md project layout
- [ ] T002 [P] Create `src/Mango.MessageBus/` class library project with MassTransit and RabbitMQ dependencies
- [ ] T003 [P] Create Auth service projects: `src/Mango.Services.AuthAPI/Mango.Services.Auth.Domain/`, `.Application/`, `.Infrastructure/`, `.API/` with project references following Clean Architecture dependency rules
- [ ] T004 [P] Create Product service projects: `src/Mango.Services.ProductAPI/Mango.Services.Product.Domain/`, `.Application/`, `.Infrastructure/`, `.API/` with project references
- [ ] T005 [P] Create ShoppingCart service projects: `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.Domain/`, `.Application/`, `.Infrastructure/`, `.API/` with project references
- [ ] T006 [P] Create Coupon service projects: `src/Mango.Services.CouponAPI/Mango.Services.Coupon.Domain/`, `.Application/`, `.Infrastructure/`, `.API/` with project references
- [ ] T007 [P] Create Order service projects: `src/Mango.Services.OrderAPI/Mango.Services.Order.Domain/`, `.Application/`, `.Infrastructure/`, `.API/` with project references
- [ ] T008 [P] Create Email service projects: `src/Mango.Services.EmailAPI/Mango.Services.Email.Domain/`, `.Application/`, `.Infrastructure/`, `.API/` with project references
- [ ] T009 [P] Create Reward service projects: `src/Mango.Services.RewardAPI/Mango.Services.Reward.Domain/`, `.Application/`, `.Infrastructure/`, `.API/` with project references
- [ ] T010 [P] Create Gateway project: `src/Mango.GatewaySolution/` with Ocelot dependency
- [ ] T011 [P] Create MVC frontend project: `src/Mango.Web/` with ASP.NET Core MVC template
- [ ] T012 [P] Create test projects: `src/tests/Mango.Services.Auth.UnitTests/`, `.IntegrationTests/`, and similarly for Product, ShoppingCart, Coupon, Order, Email, Reward, and `src/tests/Mango.ContractTests/` — all with xUnit, Moq, FluentAssertions, Testcontainers
- [ ] T013 Create `.editorconfig` at solution root enforcing naming conventions (PascalCase public, camelCase locals), nullable reference types enabled, TreatWarningsAsErrors
- [ ] T014 [P] Create `docker/docker-compose.yml` with SQL Server 2022, RabbitMQ 3.13, and Redis 7 service definitions including health checks
- [ ] T015 [P] Create `docker/docker-compose.override.yml` with development port mappings and volume mounts per quickstart.md service port map

**Checkpoint**: Solution compiles, docker-compose up starts infrastructure, all projects reference correctly.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared infrastructure that MUST be complete before ANY user story can be implemented.

**CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T016 Implement shared base entity classes in each Domain layer: `BaseEntity` with Id, CreatedAt, UpdatedAt, CreatedBy fields; `AuditableEntity` extending BaseEntity with soft delete (IsDeleted) — create in `src/Mango.Services.Auth.Domain/Entities/BaseEntity.cs` and replicate pattern per service
- [ ] T017 [P] Implement shared `ResponseDto` wrapper class with IsSuccess, Result, Message properties in each API project's DTOs (or a shared NuGet-style project) — start in `src/Mango.Services.Auth.Application/DTOs/ResponseDto.cs`
- [ ] T018 [P] Define all integration event classes in `src/Mango.MessageBus/Events/`: UserRegisteredEvent.cs, OrderPlacedEvent.cs, OrderConfirmedEvent.cs, OrderCancelledEvent.cs, CartCheckoutEvent.cs per message-bus-events.md contract
- [ ] T019 [P] Implement MassTransit registration extension methods in `src/Mango.MessageBus/Extensions/MassTransitExtensions.cs` with RabbitMQ transport, retry (3 attempts, exponential 5s/15s/45s), dead-letter, and JSON serialization
- [ ] T020 [P] Implement JWT token generation and validation shared configuration: create `JwtOptions` class and `AddJwtAuthentication` extension method reusable across all API services in `src/Mango.Services.Auth.Infrastructure/Services/JwtTokenGenerator.cs` and shared auth extension
- [ ] T021 [P] Implement Serilog structured logging configuration with JSON output, correlation ID enricher, and log level conventions in a shared pattern — configure in each API's `Program.cs`
- [ ] T022 [P] Implement OpenTelemetry tracing and metrics configuration with ASP.NET Core, EF Core, and MassTransit instrumentation — create shared setup pattern in each API's `Program.cs`
- [ ] T023 [P] Implement health check endpoints (/health/live, /health/ready) with SQL Server, RabbitMQ, and Redis checks — add `AddHealthChecks` configuration to each API's `Program.cs`
- [ ] T024 [P] Implement API versioning (URL path /api/v1/) and Swagger/OpenAPI documentation configuration — add to each API's `Program.cs`
- [ ] T025 Implement AutoMapper profiles base pattern — create mapping profile base class reusable per service in `src/Mango.Services.Auth.Application/Mappings/` and replicate pattern

**Checkpoint**: Foundation ready — all services have logging, health checks, auth middleware, messaging infrastructure, and observability. User story implementation can now begin.

---

## Phase 3: User Story 1 — Browse and Purchase Products (Priority: P1) MVP

**Goal**: Customer browses product catalog (search, filter, sort), adds items to cart, applies coupon, checks out with payment, receives email confirmation.

**Independent Test**: Navigate storefront → add product → apply coupon → checkout → verify order in history + confirmation email sent.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation.**

- [ ] T026 [P] [US1] Contract tests for Product API GET /api/v1/products (list, search, sort, pagination) and GET /api/v1/products/{id} in `src/tests/Mango.ContractTests/ProductApiContractTests.cs`
- [ ] T027 [P] [US1] Contract tests for Coupon API GET /api/v1/coupons/{code} in `src/tests/Mango.ContractTests/CouponApiContractTests.cs`
- [ ] T028 [P] [US1] Contract tests for Cart API POST /api/v1/cart/upsert, POST /api/v1/cart/apply-coupon, POST /api/v1/cart/checkout in `src/tests/Mango.ContractTests/CartApiContractTests.cs`
- [ ] T029 [P] [US1] Unit tests for Product domain entities and validation rules in `src/tests/Mango.Services.Product.UnitTests/Domain/ProductEntityTests.cs`
- [ ] T030 [P] [US1] Unit tests for Coupon domain entities and validation (expiry, min amount, discount calculation) in `src/tests/Mango.Services.Coupon.UnitTests/Domain/CouponEntityTests.cs`
- [ ] T031 [P] [US1] Unit tests for ShoppingCart domain logic (add item, remove item, recalculate total, apply/remove coupon) in `src/tests/Mango.Services.ShoppingCart.UnitTests/Domain/CartTests.cs`
- [ ] T032 [P] [US1] Integration tests for ProductRepository (CRUD, search, filter, sort, pagination) using Testcontainers SQL Server in `src/tests/Mango.Services.Product.IntegrationTests/Repositories/ProductRepositoryTests.cs`
- [ ] T033 [P] [US1] Integration tests for CouponRepository and CartRepository using Testcontainers in `src/tests/Mango.Services.Coupon.IntegrationTests/` and `src/tests/Mango.Services.ShoppingCart.IntegrationTests/`

### Implementation for User Story 1

**Product Service**:

- [ ] T034 [P] [US1] Create Category entity in `src/Mango.Services.ProductAPI/Mango.Services.Product.Domain/Entities/Category.cs` with CategoryId, Name fields
- [ ] T035 [P] [US1] Create Product entity in `src/Mango.Services.ProductAPI/Mango.Services.Product.Domain/Entities/Product.cs` with ProductId, Name, Description, Price, ImageUrl, CategoryId, IsAvailable, audit fields
- [ ] T036 [US1] Create ProductDbContext in `src/Mango.Services.ProductAPI/Mango.Services.Product.Infrastructure/Data/ProductDbContext.cs` with DbSets for Product and Category, configure indexes (CategoryId, Name, CreatedAt DESC)
- [ ] T037 [US1] Create EF Core migration for Product service initial schema and seed data (categories + sample products) in `src/Mango.Services.ProductAPI/Mango.Services.Product.Infrastructure/Data/Migrations/`
- [ ] T038 [US1] Create IProductRepository and ICategoryRepository interfaces in `src/Mango.Services.ProductAPI/Mango.Services.Product.Application/Interfaces/`
- [ ] T039 [US1] Implement ProductRepository with search (name + description LIKE), category filter, sort (price-asc/desc, name, newest), and pagination using AsNoTracking in `src/Mango.Services.ProductAPI/Mango.Services.Product.Infrastructure/Repositories/ProductRepository.cs`
- [ ] T040 [US1] Create Product DTOs (ProductDto, CreateProductDto, UpdateProductDto, ProductListResponseDto with pagination metadata) and AutoMapper profile in `src/Mango.Services.ProductAPI/Mango.Services.Product.Application/DTOs/` and `Mappings/ProductProfile.cs`
- [ ] T041 [US1] Create FluentValidation validators for CreateProductDto and UpdateProductDto in `src/Mango.Services.ProductAPI/Mango.Services.Product.Application/Validators/`
- [ ] T042 [US1] Implement ProductController with GET /api/v1/products (list+search+sort+paginate), GET /api/v1/products/{id}, POST (admin), PUT (admin), DELETE soft-delete (admin), GET /api/v1/products/categories in `src/Mango.Services.ProductAPI/Mango.Services.Product.API/Controllers/ProductController.cs`
- [ ] T043 [US1] Configure Product API Program.cs with EF Core, AutoMapper, FluentValidation, JWT auth, Serilog, OpenTelemetry, health checks, Swagger, API versioning in `src/Mango.Services.ProductAPI/Mango.Services.Product.API/Program.cs`

**Coupon Service**:

- [ ] T044 [P] [US1] Create Coupon entity in `src/Mango.Services.CouponAPI/Mango.Services.Coupon.Domain/Entities/Coupon.cs` with CouponId, CouponCode, DiscountType, DiscountAmount, MinAmount, ExpirationDate, IsActive, IsDeleted, audit fields
- [ ] T045 [US1] Create CouponDbContext with Coupon DbSet and unique index on CouponCode in `src/Mango.Services.CouponAPI/Mango.Services.Coupon.Infrastructure/Data/CouponDbContext.cs`
- [ ] T046 [US1] Create EF Core migration for Coupon service and seed sample coupons in `src/Mango.Services.CouponAPI/Mango.Services.Coupon.Infrastructure/Data/Migrations/`
- [ ] T047 [US1] Create ICouponRepository, CouponRepository (GetByCode, CRUD, soft delete) in `src/Mango.Services.CouponAPI/Mango.Services.Coupon.Application/Interfaces/ICouponRepository.cs` and `src/Mango.Services.CouponAPI/Mango.Services.Coupon.Infrastructure/Repositories/CouponRepository.cs`
- [ ] T048 [US1] Create Coupon DTOs, AutoMapper profile, FluentValidation validators in `src/Mango.Services.CouponAPI/Mango.Services.Coupon.Application/DTOs/`, `Mappings/`, `Validators/`
- [ ] T049 [US1] Implement CouponController with GET /api/v1/coupons/{code} (public), GET /api/v1/coupons (admin list), POST (admin), PUT (admin), DELETE soft-delete (admin) in `src/Mango.Services.CouponAPI/Mango.Services.Coupon.API/Controllers/CouponController.cs`
- [ ] T050 [US1] Configure Coupon API Program.cs with full middleware stack in `src/Mango.Services.CouponAPI/Mango.Services.Coupon.API/Program.cs`

**Shopping Cart Service**:

- [ ] T051 [P] [US1] Create CartHeader and CartDetails entities in `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.Domain/Entities/CartHeader.cs` and `CartDetails.cs` per data-model.md
- [ ] T052 [US1] Create CartDbContext with DbSets, unique index on UserId for CartHeader in `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.Infrastructure/Data/CartDbContext.cs`
- [ ] T053 [US1] Create EF Core migration for Cart service in `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.Infrastructure/Data/Migrations/`
- [ ] T054 [US1] Create ICartRepository with GetByUserId, Upsert, RemoveItem, ApplyCoupon, RemoveCoupon, ClearCart methods in `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.Application/Interfaces/ICartRepository.cs`
- [ ] T055 [US1] Implement CartRepository in `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.Infrastructure/Repositories/CartRepository.cs`
- [ ] T056 [US1] Create Cart DTOs, AutoMapper profile in `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.Application/DTOs/` and `Mappings/`
- [ ] T057 [US1] Implement IProductService and ICouponService HTTP client interfaces for cross-service calls in `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.Application/Interfaces/` with HttpClientFactory implementations in Infrastructure
- [ ] T058 [US1] Implement CartController with GET /api/v1/cart, POST /api/v1/cart/upsert, DELETE /api/v1/cart/remove/{id}, POST apply-coupon, POST remove-coupon in `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.API/Controllers/CartController.cs`
- [ ] T059 [US1] Implement POST /api/v1/cart/checkout endpoint that validates cart, verifies prices with Product service, creates Stripe checkout session, publishes CartCheckoutEvent via MassTransit outbox in `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.API/Controllers/CartController.cs`
- [ ] T060 [US1] Configure Cart API Program.cs with full middleware stack plus MassTransit publisher and HttpClientFactory in `src/Mango.Services.ShoppingCartAPI/Mango.Services.ShoppingCart.API/Program.cs`

**Checkpoint**: Product catalog browsable (search/filter/sort), coupons validatable, cart fully functional with checkout initiating payment. US1 core backend complete.

---

## Phase 4: User Story 2 — User Registration and Authentication (Priority: P2)

**Goal**: Visitors register, log in with JWT, session persists, admins manage roles. Password policy and lockout enforced.

**Independent Test**: Register → login → verify JWT → navigate pages → admin assign role → verify permissions.

### Tests for User Story 2

- [ ] T061 [P] [US2] Contract tests for Auth API POST /register, POST /login, POST /refresh, POST /assign-role in `src/tests/Mango.ContractTests/AuthApiContractTests.cs`
- [ ] T062 [P] [US2] Unit tests for JwtTokenGenerator, password validation (8+ chars, 1 upper, 1 number, 1 special), lockout logic (5 attempts, 15 min) in `src/tests/Mango.Services.Auth.UnitTests/Services/`
- [ ] T063 [P] [US2] Integration tests for Auth registration, login, token refresh, role assignment flows using Testcontainers in `src/tests/Mango.Services.Auth.IntegrationTests/`

### Implementation for User Story 2

- [ ] T064 [P] [US2] Create ApplicationUser entity extending IdentityUser with Name, FailedLoginAttempts, LockoutEndUtc in `src/Mango.Services.AuthAPI/Mango.Services.Auth.Domain/Entities/ApplicationUser.cs`
- [ ] T065 [P] [US2] Create RefreshToken entity in `src/Mango.Services.AuthAPI/Mango.Services.Auth.Domain/Entities/RefreshToken.cs`
- [ ] T066 [US2] Create AuthDbContext extending IdentityDbContext with RefreshToken DbSet in `src/Mango.Services.AuthAPI/Mango.Services.Auth.Infrastructure/Data/AuthDbContext.cs`
- [ ] T067 [US2] Create EF Core migration for Auth service with Identity tables + RefreshToken, seed Admin and Customer roles in `src/Mango.Services.AuthAPI/Mango.Services.Auth.Infrastructure/Data/Migrations/`
- [ ] T068 [US2] Implement IAuthService interface with Register, Login, RefreshToken, AssignRole methods in `src/Mango.Services.AuthAPI/Mango.Services.Auth.Application/Interfaces/IAuthService.cs`
- [ ] T069 [US2] Implement IJwtTokenGenerator with access token (30 min) and refresh token (7 days) generation in `src/Mango.Services.AuthAPI/Mango.Services.Auth.Infrastructure/Services/JwtTokenGenerator.cs`
- [ ] T070 [US2] Implement AuthService with password complexity validation (FluentValidation), Identity registration, login with lockout (5 attempts / 15 min), token refresh, role assignment in `src/Mango.Services.AuthAPI/Mango.Services.Auth.Infrastructure/Services/AuthService.cs`
- [ ] T071 [US2] Create Auth DTOs (RegisterRequestDto, LoginRequestDto, LoginResponseDto, AssignRoleDto, RefreshTokenDto) and validators in `src/Mango.Services.AuthAPI/Mango.Services.Auth.Application/DTOs/` and `Validators/`
- [ ] T072 [US2] Implement AuthController with POST /register, POST /login, POST /refresh, POST /assign-role in `src/Mango.Services.AuthAPI/Mango.Services.Auth.API/Controllers/AuthController.cs`
- [ ] T073 [US2] Publish UserRegisteredEvent via MassTransit outbox on successful registration in AuthService in `src/Mango.Services.AuthAPI/Mango.Services.Auth.Infrastructure/Services/AuthService.cs`
- [ ] T074 [US2] Configure Auth API Program.cs with Identity, EF Core, JWT, MassTransit publisher, Serilog, OpenTelemetry, health checks, Swagger in `src/Mango.Services.AuthAPI/Mango.Services.Auth.API/Program.cs`

**Checkpoint**: Users can register, login, refresh tokens. Admins assign roles. Password policy and lockout enforced. UserRegisteredEvent published.

---

## Phase 5: User Story 3 — Order Management and History (Priority: P3)

**Goal**: Orders created from checkout, status tracked through linear state machine, customers view order history with details, cancellation by customer or admin from Pending/Confirmed.

**Independent Test**: Complete checkout → order appears in history → admin updates status → customer views updated status → cancel from Pending.

### Tests for User Story 3

- [ ] T075 [P] [US3] Contract tests for Order API POST /create, GET /orders, GET /orders/{id}, POST /update-status, POST /cancel in `src/tests/Mango.ContractTests/OrderApiContractTests.cs`
- [ ] T076 [P] [US3] Unit tests for Order domain: status state machine (valid transitions only), cancellation rules (Pending/Confirmed only, by customer or admin), total calculation in `src/tests/Mango.Services.Order.UnitTests/Domain/OrderStatusTests.cs`
- [ ] T077 [P] [US3] Integration tests for Order creation from CartCheckoutEvent, status updates, Stripe webhook validation using Testcontainers in `src/tests/Mango.Services.Order.IntegrationTests/`

### Implementation for User Story 3

- [ ] T078 [P] [US3] Create OrderHeader entity with status state machine (Pending→Confirmed→Processing→Shipped→Delivered, cancel from Pending/Confirmed) in `src/Mango.Services.OrderAPI/Mango.Services.Order.Domain/Entities/OrderHeader.cs`
- [ ] T079 [P] [US3] Create OrderDetails entity in `src/Mango.Services.OrderAPI/Mango.Services.Order.Domain/Entities/OrderDetails.cs`
- [ ] T080 [US3] Create OrderDbContext with DbSets, OrderHeader status index in `src/Mango.Services.OrderAPI/Mango.Services.Order.Infrastructure/Data/OrderDbContext.cs`
- [ ] T081 [US3] Create EF Core migration for Order service in `src/Mango.Services.OrderAPI/Mango.Services.Order.Infrastructure/Data/Migrations/`
- [ ] T082 [US3] Create IOrderRepository with Create, GetByUserId (paginated), GetById, UpdateStatus, Cancel methods in `src/Mango.Services.OrderAPI/Mango.Services.Order.Application/Interfaces/IOrderRepository.cs`
- [ ] T083 [US3] Implement OrderRepository in `src/Mango.Services.OrderAPI/Mango.Services.Order.Infrastructure/Repositories/OrderRepository.cs`
- [ ] T084 [US3] Create Order DTOs, AutoMapper profile, FluentValidation validators in `src/Mango.Services.OrderAPI/Mango.Services.Order.Application/DTOs/`, `Mappings/`, `Validators/`
- [ ] T085 [US3] Implement CartCheckoutEventConsumer (creates order from checkout event) in `src/Mango.Services.OrderAPI/Mango.Services.Order.Infrastructure/Consumers/CartCheckoutEventConsumer.cs`
- [ ] T086 [US3] Implement Stripe webhook endpoint POST /validate-stripe that confirms payment and transitions order to Confirmed, publishes OrderConfirmedEvent in `src/Mango.Services.OrderAPI/Mango.Services.Order.API/Controllers/OrderController.cs`
- [ ] T087 [US3] Implement OrderController with POST /create, GET /orders (customer: own orders, admin: all), GET /orders/{id}, POST /update-status (admin), POST /cancel (customer+admin, Pending/Confirmed only) in `src/Mango.Services.OrderAPI/Mango.Services.Order.API/Controllers/OrderController.cs`
- [ ] T088 [US3] Publish OrderPlacedEvent, OrderConfirmedEvent, OrderCancelledEvent via MassTransit outbox from Order service in `src/Mango.Services.OrderAPI/Mango.Services.Order.Infrastructure/`
- [ ] T089 [US3] Configure Order API Program.cs with full middleware stack plus MassTransit consumer and publisher in `src/Mango.Services.OrderAPI/Mango.Services.Order.API/Program.cs`

**Checkpoint**: Orders are created from checkout events, status tracked through state machine, history viewable, cancellation works. Events published for downstream consumers.

---

## Phase 6: User Story 4 — Loyalty Rewards Program (Priority: P4)

**Goal**: Points credited automatically on order confirmation (1 point per $1 before discounts). Balance and history viewable on profile.

**Independent Test**: Place order → order confirmed → verify points credited → view balance and activity history.

### Tests for User Story 4

- [ ] T090 [P] [US4] Contract tests for Reward API GET /rewards and GET /rewards/balance in `src/tests/Mango.ContractTests/RewardApiContractTests.cs`
- [ ] T091 [P] [US4] Unit tests for points calculation (1:1 ratio, round nearest, before-discount amount) in `src/tests/Mango.Services.Reward.UnitTests/Domain/RewardsCalculationTests.cs`
- [ ] T092 [P] [US4] Integration tests for OrderConfirmedEvent consumer (points credited, idempotent) using Testcontainers in `src/tests/Mango.Services.Reward.IntegrationTests/`

### Implementation for User Story 4

- [ ] T093 [P] [US4] Create RewardsBalance and RewardsActivity entities in `src/Mango.Services.RewardAPI/Mango.Services.Reward.Domain/Entities/RewardsBalance.cs` and `RewardsActivity.cs`
- [ ] T094 [US4] Create RewardDbContext with DbSets, unique index on UserId for RewardsBalance in `src/Mango.Services.RewardAPI/Mango.Services.Reward.Infrastructure/Data/RewardDbContext.cs`
- [ ] T095 [US4] Create EF Core migration for Reward service in `src/Mango.Services.RewardAPI/Mango.Services.Reward.Infrastructure/Data/Migrations/`
- [ ] T096 [US4] Create IRewardsRepository with GetByUserId, CreditPoints, GetActivityHistory methods in `src/Mango.Services.RewardAPI/Mango.Services.Reward.Application/Interfaces/IRewardsRepository.cs`
- [ ] T097 [US4] Implement RewardsRepository in `src/Mango.Services.RewardAPI/Mango.Services.Reward.Infrastructure/Repositories/RewardsRepository.cs`
- [ ] T098 [US4] Implement OrderConfirmedEventConsumer (calculate points = round(orderTotalBeforeDiscount), credit to balance, create activity record, idempotent by OrderId) in `src/Mango.Services.RewardAPI/Mango.Services.Reward.Infrastructure/Consumers/OrderConfirmedEventConsumer.cs`
- [ ] T099 [US4] Create Reward DTOs and AutoMapper profile in `src/Mango.Services.RewardAPI/Mango.Services.Reward.Application/DTOs/` and `Mappings/`
- [ ] T100 [US4] Implement RewardController with GET /api/v1/rewards (balance + activity history) and GET /api/v1/rewards/balance in `src/Mango.Services.RewardAPI/Mango.Services.Reward.API/Controllers/RewardController.cs`
- [ ] T101 [US4] Configure Reward API Program.cs with full middleware stack plus MassTransit consumer in `src/Mango.Services.RewardAPI/Mango.Services.Reward.API/Program.cs`

**Checkpoint**: Rewards points auto-credited on order confirmation. Balance and activity history viewable.

---

## Phase 7: User Story 5 — Coupon and Promotion Management (Priority: P5)

**Goal**: Admin creates, updates, deactivates coupons. Customer coupon application covered in US1; this phase adds admin CRUD management and validation.

**Independent Test**: Admin creates coupon → customer applies it → admin deactivates → customer sees rejection.

### Tests for User Story 5

- [ ] T102 [P] [US5] Unit tests for coupon validation logic (expired, inactive, min amount, discount type calculation) in `src/tests/Mango.Services.Coupon.UnitTests/Domain/CouponValidationTests.cs`
- [ ] T103 [P] [US5] Integration tests for admin coupon CRUD (create, update, deactivate, list with pagination) in `src/tests/Mango.Services.Coupon.IntegrationTests/Controllers/CouponAdminTests.cs`

### Implementation for User Story 5

- [ ] T104 [US5] Add coupon validation domain logic: IsExpired(), IsEligible(cartTotal), CalculateDiscount(cartTotal) methods to Coupon entity in `src/Mango.Services.CouponAPI/Mango.Services.Coupon.Domain/Entities/Coupon.cs`
- [ ] T105 [US5] Add admin-only coupon listing endpoint with pagination and includeInactive filter to CouponController in `src/Mango.Services.CouponAPI/Mango.Services.Coupon.API/Controllers/CouponController.cs`
- [ ] T106 [US5] Add coupon update (PUT) and deactivation (DELETE soft-delete) endpoints with admin role authorization to CouponController in `src/Mango.Services.CouponAPI/Mango.Services.Coupon.API/Controllers/CouponController.cs`

**Checkpoint**: Full admin coupon lifecycle management. Customer coupon application validated server-side.

---

## Phase 8: User Story 6 — Email Notifications (Priority: P6)

**Goal**: Asynchronous transactional emails for registration welcome and order confirmation. Emails queued for retry on failure, never block the user.

**Independent Test**: Register → welcome email sent. Place order → confirmation email sent. Email service down → email queued → service recovers → email delivered.

### Tests for User Story 6

- [ ] T107 [P] [US6] Unit tests for email template rendering (welcome, order confirmation) in `src/tests/Mango.Services.Email.UnitTests/Services/EmailTemplateTests.cs`
- [ ] T108 [P] [US6] Integration tests for UserRegisteredEvent and OrderConfirmedEvent consumers (email logged, idempotent) using Testcontainers in `src/tests/Mango.Services.Email.IntegrationTests/`

### Implementation for User Story 6

- [ ] T109 [P] [US6] Create EmailLog entity in `src/Mango.Services.EmailAPI/Mango.Services.Email.Domain/Entities/EmailLog.cs` with fields per data-model.md
- [ ] T110 [US6] Create EmailDbContext with EmailLog DbSet in `src/Mango.Services.EmailAPI/Mango.Services.Email.Infrastructure/Data/EmailDbContext.cs`
- [ ] T111 [US6] Create EF Core migration for Email service in `src/Mango.Services.EmailAPI/Mango.Services.Email.Infrastructure/Data/Migrations/`
- [ ] T112 [US6] Create IEmailSender interface and implementation (SMTP or SendGrid abstraction) with retry logic in `src/Mango.Services.EmailAPI/Mango.Services.Email.Application/Interfaces/IEmailSender.cs` and `src/Mango.Services.EmailAPI/Mango.Services.Email.Infrastructure/Services/EmailSender.cs`
- [ ] T113 [US6] Implement UserRegisteredEventConsumer (send welcome email, log to EmailLog, idempotent by UserId) in `src/Mango.Services.EmailAPI/Mango.Services.Email.Infrastructure/Consumers/UserRegisteredEventConsumer.cs`
- [ ] T114 [US6] Implement OrderConfirmedEventConsumer (send order confirmation email with details, log to EmailLog, idempotent by OrderId) in `src/Mango.Services.EmailAPI/Mango.Services.Email.Infrastructure/Consumers/OrderConfirmedEventConsumer.cs`
- [ ] T115 [US6] Implement EmailController with GET /api/v1/email/logs (admin), POST /api/v1/email/retry/{id} (admin) in `src/Mango.Services.EmailAPI/Mango.Services.Email.API/Controllers/EmailController.cs`
- [ ] T116 [US6] Configure Email API Program.cs with MassTransit consumers, EF Core, health checks, Serilog in `src/Mango.Services.EmailAPI/Mango.Services.Email.API/Program.cs`

**Checkpoint**: Transactional emails sent asynchronously for registration and order confirmation. Failed emails logged and retryable.

---

## Phase 9: User Story 7 — API Gateway and Frontend Integration (Priority: P7)

**Goal**: All frontend requests route through Ocelot gateway. Gateway validates JWT, routes to correct service, handles errors gracefully. MVC frontend provides full user-facing storefront.

**Independent Test**: Frontend sends request through gateway → correctly routed → response displayed. Backend down → gateway returns friendly error.

### Tests for User Story 7

- [ ] T117 [P] [US7] Integration tests for gateway routing: auth, product, cart, coupon, order, reward routes correctly forwarded in `src/tests/Mango.ContractTests/GatewayRoutingTests.cs`

### Implementation for User Story 7

- [ ] T118 [US7] Configure Ocelot routing in `src/Mango.GatewaySolution/ocelot.json` with routes for all 7 backend services per quickstart.md port map
- [ ] T119 [US7] Configure Gateway Program.cs with Ocelot, JWT authentication forwarding, Serilog, health checks in `src/Mango.GatewaySolution/Program.cs`
- [ ] T120 [US7] Create HTTP client service interfaces in `src/Mango.Web/Services/` for auth (IAuthService), product (IProductService), cart (ICartService), coupon (ICouponService), order (IOrderService), reward (IRewardService) calling gateway endpoints
- [ ] T121 [US7] Implement HTTP client services with HttpClientFactory, JWT token attachment from session cookie, error handling in `src/Mango.Web/Services/`
- [ ] T122 [US7] Create MVC Models (ViewModels) for product listing, product detail, cart, checkout, order history, login, register, rewards in `src/Mango.Web/Models/`
- [ ] T123 [US7] Implement HomeController with product listing (search, filter, sort), product detail page in `src/Mango.Web/Controllers/HomeController.cs` with Views in `src/Mango.Web/Views/Home/`
- [ ] T124 [US7] Implement AuthController with register and login pages, JWT cookie management in `src/Mango.Web/Controllers/AuthController.cs` with Views
- [ ] T125 [US7] Implement CartController with cart view, add/remove items, apply/remove coupon, checkout flow in `src/Mango.Web/Controllers/CartController.cs` with Views
- [ ] T126 [US7] Implement OrderController with order history list and order detail pages in `src/Mango.Web/Controllers/OrderController.cs` with Views
- [ ] T127 [US7] Implement shared layout (_Layout.cshtml) with navigation, login/logout, cart icon with item count in `src/Mango.Web/Views/Shared/`
- [ ] T128 [US7] Configure Mango.Web Program.cs with MVC, HttpClientFactory (gateway base URL), cookie authentication, session management in `src/Mango.Web/Program.cs`

**Checkpoint**: Complete frontend storefront operational through API gateway. All user flows (browse, register, login, cart, checkout, order history, rewards) accessible via web UI.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories.

- [ ] T129 [P] Create multi-stage Dockerfiles for each service in `docker/` optimized for minimal image size (.NET Alpine SDK build, ASP.NET runtime-deps production, non-root user)
- [ ] T130 [P] Create Kubernetes manifests (Deployment, Service, ConfigMap, Secret, HPA) for each microservice in `k8s/` directories per plan.md structure
- [ ] T131 [P] Create `k8s/infrastructure/` manifests for SQL Server, RabbitMQ, and Redis StatefulSets
- [ ] T132 [P] Implement Redis distributed caching for product catalog (5-min TTL) with event-driven invalidation on ProductUpdated in Product service
- [ ] T133 [P] Implement Redis distributed caching for coupon validation (1-min TTL) with event-driven invalidation in Coupon service
- [ ] T134 [P] Add correlation ID middleware to all API services propagating trace context through HTTP headers and MassTransit message headers
- [ ] T135 Code cleanup: ensure all services consistently use async/await throughout, no sync-over-async patterns, HttpClientFactory for all HTTP clients
- [ ] T136 [P] Run quickstart.md end-to-end validation: start all services via docker-compose, execute smoke test sequence (register → login → browse → cart → checkout → order history → rewards)
- [ ] T137 Security hardening: verify all endpoints have explicit [Authorize] or [AllowAnonymous], HTTPS enforcement in non-dev, secrets in environment variables
- [ ] T138 [P] Add comprehensive Swagger XML documentation comments to all controllers and DTOs across all services

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational — MVP, implement first
- **US2 (Phase 4)**: Depends on Foundational — can parallel with US1 (different services)
- **US3 (Phase 5)**: Depends on US1 (checkout creates orders) + US2 (auth for history)
- **US4 (Phase 6)**: Depends on US3 (OrderConfirmedEvent triggers points)
- **US5 (Phase 7)**: Depends on US1 (coupon entity created in US1)
- **US6 (Phase 8)**: Depends on US2 (UserRegisteredEvent) + US3 (OrderConfirmedEvent)
- **US7 (Phase 9)**: Depends on US1 + US2 (needs backend services to route to)
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1 (Setup) → Phase 2 (Foundational)
                       ↓
              ┌────────┴────────┐
              ↓                 ↓
         US1 (Phase 3)    US2 (Phase 4)
              ↓         ↗       ↓
         US3 (Phase 5) ←────────┘
              ↓
     ┌────────┼────────┐
     ↓        ↓        ↓
US4 (Ph6) US5 (Ph7) US6 (Ph8)
     ↓        ↓        ↓
     └────────┼────────┘
              ↓
         US7 (Phase 9)
              ↓
        Polish (Phase 10)
```

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Entities before DbContext
- DbContext before migrations
- Repositories (interface then implementation) before services
- Services before controllers
- Controller implementation before Program.cs configuration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks (T002-T015) marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel
- US1 and US2 can run in parallel (different services)
- US4, US5, US6 can run in parallel after US3
- All test tasks within a story marked [P] can run in parallel
- Entity creation within a story marked [P] can run in parallel
- All Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all tests for US1 together:
Task: "Contract tests for Product API" (T026)
Task: "Contract tests for Coupon API" (T027)
Task: "Contract tests for Cart API" (T028)
Task: "Unit tests for Product domain" (T029)
Task: "Unit tests for Coupon domain" (T030)
Task: "Unit tests for Cart domain" (T031)
Task: "Integration tests Product repo" (T032)
Task: "Integration tests Coupon + Cart repos" (T033)

# Launch all entities for US1 together:
Task: "Create Category entity" (T034)
Task: "Create Product entity" (T035)
Task: "Create Coupon entity" (T044)
Task: "Create CartHeader + CartDetails entities" (T051)
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: US1 (Browse + Purchase) — in parallel with:
4. Complete Phase 4: US2 (Auth) — parallel with US1
5. **STOP and VALIDATE**: Both stories independently testable
6. Deploy/demo if ready — core e-commerce flow operational

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 + US2 (parallel) → Core MVP (browse, auth, cart, checkout)
3. US3 → Order management + history
4. US4 + US5 + US6 (parallel) → Rewards + Coupon admin + Emails
5. US7 → Gateway + Frontend integration
6. Polish → Docker, K8s, caching, security hardening

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Product + Coupon + Cart)
   - Developer B: US2 (Auth)
3. After US1 + US2:
   - Developer A: US3 (Order)
   - Developer B: US7 (Gateway + Frontend)
4. After US3:
   - Developer A: US4 (Rewards)
   - Developer B: US5 (Coupon admin) + US6 (Email)
5. Both: Polish phase

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Constitution Principle V mandates tests — all stories include test tasks
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
