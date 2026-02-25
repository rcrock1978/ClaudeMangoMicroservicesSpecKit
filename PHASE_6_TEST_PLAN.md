# Phase 6: Testing & Quality Assurance Plan

**Start Date**: February 25, 2026
**Duration**: 2-3 weeks
**Status**: 🚀 **IN PROGRESS**

---

## 🎯 Phase 6 Objectives

1. **Integration Testing** - Verify service-to-service communication
2. **Performance Testing** - Validate response times and throughput
3. **Security Testing** - Ensure authentication and authorization work
4. **E2E Testing** - Complete user workflow validation

---

## 📋 Test Structure

```
tests/
├── Integration/
│   ├── Mango.Services.Admin.Accounts.IntegrationTests/
│   │   ├── Fixtures/
│   │   │   └── ApiTestFixture.cs         ✅ Created
│   │   └── Tests/
│   │       └── AdminAccountsControllerTests.cs  ✅ Created
│   │
│   └── Mango.Services.Admin.IntegrationTests/
│       ├── Fixtures/
│       │   └── AdminApiTestFixture.cs    ✅ Created
│       └── Tests/
│           └── DashboardControllerTests.cs      ✅ Created
│
├── E2E/
│   └── ApiGatewayE2ETests.cs            📋 Planned
│
└── Performance/
    └── AdminServiceLoadTests.cs         📋 Planned
```

---

## ✅ Test Coverage Matrix

### Admin.Accounts Service Tests

| Feature | Test Case | Status | Expected Result |
|---------|-----------|--------|-----------------|
| **Health Check** | `/health` returns 200 | ✅ | OK response |
| **API Key Validation** | Valid key returns user info | 📋 | 200 OK |
| **API Key Validation** | Invalid key returns 401 | ✅ | Unauthorized |
| **API Key Validation** | Empty key returns 401 | ✅ | Unauthorized |
| **Create Admin User** | Without API key returns 401 | ✅ | Unauthorized |
| **Create Admin User** | Duplicate email returns 400 | 📋 | Bad Request |
| **Create Admin User** | Valid creates user | 📋 | 201 Created |
| **Get Admin User** | Valid ID returns user | 📋 | 200 OK |
| **Get Admin User** | Invalid ID returns 404 | 📋 | Not Found |
| **List Admin Users** | Pagination works | 📋 | 200 OK with data |
| **Update Admin User** | Updates only allowed fields | 📋 | 200 OK |
| **Deactivate Admin User** | Sets IsActive to false | 📋 | 200 OK |
| **Create API Key** | Returns one-time plain text | 📋 | 201 Created |
| **Create API Key** | Revokes previous key | 📋 | Previous key disabled |
| **Revoke API Key** | Sets IsRevoked to true | 📋 | 200 OK |

### Admin Service Dashboard Tests

| Feature | Test Case | Status | Expected Result |
|---------|-----------|--------|-----------------|
| **Health Check** | `/health` returns 200 | ✅ | OK response |
| **Dashboard KPIs** | Requires authentication | ✅ | 401 Unauthorized |
| **Revenue Metrics** | Requires authentication | ✅ | 401 Unauthorized |
| **Product Metrics** | Requires authentication | ✅ | 401 Unauthorized |
| **Customer Insights** | Requires authentication | ✅ | 401 Unauthorized |
| **Coupon Analytics** | Requires authentication | ✅ | 401 Unauthorized |
| **KPI Caching** | 5-minute cache works | 📋 | Data consistency |
| **Data Aggregation** | Calls all 5 services | 📋 | Complete data |
| **Response Format** | DashboardKpiDto structure | 📋 | Valid JSON |

### API Gateway Tests

| Feature | Test Case | Status | Expected Result |
|---------|-----------|--------|-----------------|
| **Route Aggregation** | `/api/admin/accounts/*` routes correctly | 📋 | Forwarded to 5094 |
| **Route Aggregation** | `/api/admin/dashboard/*` routes correctly | 📋 | Forwarded to 5095 |
| **Health Check** | Gateway health endpoint | 📋 | 200 OK |
| **CORS Headers** | Allows cross-origin requests | 📋 | Correct headers |
| **Service Discovery** | DNS resolution works | 📋 | Services found |
| **Request Forwarding** | Headers preserved | 📋 | API key passed through |

---

## 🧪 Test Types & Priorities

### Tier 1: Critical Path Tests (High Priority)
- ✅ Health check endpoints
- ✅ Authentication/Authorization
- ✅ API contract validation
- ⏳ Happy path workflows

### Tier 2: Feature Tests (Medium Priority)
- ⏳ API key creation & validation
- ⏳ Admin user CRUD operations
- ⏳ Data aggregation
- ⏳ Pagination & filtering

### Tier 3: Edge Cases (Medium Priority)
- ⏳ Invalid input handling
- ⏳ Duplicate resource prevention
- ⏳ Concurrent requests
- ⏳ Rate limiting

### Tier 4: Performance Tests (Lower Priority)
- ⏳ Response time < 500ms
- ⏳ Throughput > 100 req/s
- ⏳ Cache effectiveness
- ⏳ Database connection pooling

---

## 📊 Test Metrics & Goals

| Metric | Target | Current | Status |
|--------|--------|---------|--------|
| **Code Coverage** | > 80% | TBD | 📋 |
| **Critical Tests** | 100% passing | 8/8 | ✅ 100% |
| **Feature Tests** | > 90% passing | TBD | 📋 |
| **Test Execution Time** | < 30 seconds | TBD | 📋 |
| **P99 Response Time** | < 500ms | TBD | 📋 |
| **Availability (3 runs)** | 99.9% | TBD | 📋 |

---

## 🔄 Testing Workflow

### Weekly Cycle
```
Monday     - Plan new tests
Tuesday    - Implement test code
Wednesday  - Code review & fixes
Thursday   - Run full test suite
Friday     - Performance testing & reporting
```

### Per Feature
```
Feature Branch
    ↓
Write Tests
    ↓
Implement Code
    ↓
Unit Tests Pass
    ↓
Integration Tests Pass
    ↓
Code Review
    ↓
Merge to Main
```

---

## 🛠️ Testing Tools & Configuration

### Unit Testing
- **Framework**: xUnit
- **Mocking**: Moq
- **Assertions**: FluentAssertions
- **Command**: `dotnet test`

### Integration Testing
- **API Testing**: WebApplicationFactory
- **Database**: In-memory (testing)
- **Command**: `dotnet test tests/Integration/`

### Performance Testing
- **Tool**: Apache JMeter or k6
- **Scenarios**: Load, stress, endurance
- **Metrics**: Response time, throughput, errors

### Security Testing
- **Method**: Manual + automated
- **Focus**: Authentication, Authorization, input validation
- **Tools**: Postman, Burp Suite (optional)

---

## 📝 Test Implementation Checklist

### Setup (✅ In Progress)
- [x] Create test project structure
- [x] Add xUnit & testing dependencies
- [x] Create API fixtures
- [ ] Configure appsettings.Testing.json
- [ ] Setup CI/CD test pipeline

### Admin.Accounts Tests (✅ Started)
- [x] Health check tests
- [x] Authentication tests
- [ ] Create admin user tests
- [ ] API key management tests
- [ ] Error handling tests
- [ ] Pagination tests

### Admin Service Tests (✅ Started)
- [x] Health check tests
- [x] Dashboard endpoint security tests
- [ ] KPI aggregation tests
- [ ] Cache validation tests
- [ ] Data freshness tests
- [ ] Error handling tests

### API Gateway Tests (📋 Pending)
- [ ] Route forwarding tests
- [ ] CORS tests
- [ ] Service discovery tests
- [ ] Error handling tests
- [ ] Performance tests

### E2E Tests (📋 Pending)
- [ ] Complete user workflows
- [ ] Multi-service interactions
- [ ] Docker Compose validation
- [ ] Database persistence

### Performance Tests (📋 Pending)
- [ ] Load testing (100+ concurrent users)
- [ ] Stress testing (breaking point)
- [ ] Endurance testing (24-hour run)
- [ ] Response time profiling

---

## 🔍 Quality Gates

Tests must pass before merging:

```
Code Must:
  ✅ Compile without errors
  ✅ Pass all unit tests
  ✅ Pass integration tests
  ✅ Maintain > 80% code coverage
  ✅ Have no critical warnings

Build Must:
  ✅ Complete in < 2 minutes
  ✅ Produce deployable artifacts
  ✅ Pass linting checks
```

---

## 📋 Test Scenarios

### Scenario 1: Admin Registration Workflow
1. Call ValidateApiKey with bootstrap key
2. Call CreateAdminUser endpoint
3. Verify user created in database
4. Call GenerateApiKey endpoint
5. Verify new API key works

### Scenario 2: Dashboard Access Flow
1. Obtain valid API key
2. Call GetDashboardKpis
3. Verify all 5 services aggregated
4. Check cache is used on second call
5. Verify response time < 500ms

### Scenario 3: Authorization Enforcement
1. Call endpoint without API key → 401
2. Call endpoint with invalid key → 401
3. Call with valid key but insufficient role → 403
4. Call with valid key and role → 200

### Scenario 4: Error Handling
1. Send invalid JSON → 400
2. Send missing required fields → 400
3. Send duplicate email → 400
4. Trigger database error → 500
5. Verify error message sanitized

---

## 🚀 Running Tests

### Run All Tests
```bash
cd tests
dotnet test
```

### Run Specific Project
```bash
dotnet test tests/Integration/Mango.Services.Admin.Accounts.IntegrationTests/
dotnet test tests/Integration/Mango.Services.Admin.IntegrationTests/
```

### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Run Performance Tests
```bash
# Using JMeter
jmeter -n -t admin-service-load-test.jmx -l results.jtl

# Using k6
k6 run admin-service-load-test.js
```

---

## 📊 Test Results Template

```markdown
# Test Run Results - [Date]

## Summary
- Total Tests: X
- Passed: X ✅
- Failed: 0
- Skipped: 0
- Coverage: X%

## By Category
- Admin.Accounts: X/X passing
- Admin.Service: X/X passing
- Gateway: X/X passing
- E2E: X/X passing

## Performance
- Avg Response Time: Xms
- P99 Response Time: Xms
- Throughput: X req/s
- Error Rate: X%

## Issues Found
- None ✅

## Next Steps
- [ ] Deploy to staging
- [ ] Monitor production
```

---

## 👥 Testing Responsibilities

| Role | Responsibility |
|------|---|
| **QA Lead** | Test planning, coverage tracking |
| **Developers** | Unit tests, integration tests |
| **DevOps** | Performance tests, environment setup |
| **Tech Lead** | Test architecture, quality gates |

---

## 📅 Phase 6 Timeline

```
Week 1: Core Integration Tests
  Mon: Test planning & setup
  Tue-Wed: Admin.Accounts tests
  Thu-Fri: Admin.Service tests

Week 2: Advanced Testing
  Mon-Tue: API Gateway tests
  Wed-Thu: E2E test scenarios
  Fri: Performance testing

Week 3: Optimization & Review
  Mon-Tue: Fix failing tests
  Wed-Thu: Security testing
  Fri: Report & sign-off
```

---

## ✨ Success Criteria

Phase 6 is complete when:
- [ ] 80%+ code coverage achieved
- [ ] All critical tests passing
- [ ] No critical security issues found
- [ ] Performance targets met
- [ ] Documentation complete
- [ ] Team sign-off obtained

---

**Next Phase**: Phase 7 - Kubernetes Deployment

---

**Document Created**: February 25, 2026
**Last Updated**: February 25, 2026
**Status**: 🚀 In Progress
