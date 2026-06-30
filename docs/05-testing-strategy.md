# Testing Strategy

## Purpose

This document defines the testing strategy for SilverHub.

The goal is to keep the project production-ready while using tests as a way to learn and retain implementation details.

The project should not chase 100% coverage. It should have meaningful coverage around domain logic, API behavior, database integration, and critical frontend flows.

## Testing principles

- Test behavior, not implementation details.
- Prioritize high-risk code.
- Prefer fast tests where possible.
- Use real PostgreSQL for database integration tests.
- Keep end-to-end tests small and valuable.
- Every bug fix should usually add or update a test.
- Tests should be readable enough to teach the feature.

## Test pyramid

Target shape:

```text
Many:
  unit tests

Some:
  integration tests

Few:
  end-to-end smoke tests
```

Avoid an inverted pyramid where most tests require a full browser and deployed app.

## Backend unit tests

### Project

```text
tests/SilverHub.Domain.Tests
tests/SilverHub.Application.Tests
```

### Tools

- xUnit
- FluentAssertions or built-in assertions
- coverlet for coverage reporting

### What to test

Domain tests:

- Gear optimization solver.
- BP deficit calculation.
- Slug normalization.
- Team builder payload/domain rules.
- Value objects.
- Any ranking/scoring logic.
- Edge cases around game formulas.

Application tests:

- Query handlers.
- Command handlers.
- Validation behavior.
- Cache decision logic.
- Not-found behavior.
- Repository port interactions with fakes.

### What not to test heavily

- Trivial DTO property assignment.
- Framework behavior.
- Simple one-line pass-through methods unless they are important use cases.
- Private methods directly.

## Backend integration tests

### Project

```text
tests/SilverHub.Infrastructure.Tests
tests/SilverHub.Api.Tests
```

### Tools

- xUnit
- Microsoft.AspNetCore.Mvc.Testing
- Testcontainers for PostgreSQL
- Respawn or database reset helper, if useful

### Why real PostgreSQL

The production database is PostgreSQL. Integration tests should catch provider-specific behavior such as:

- JSON column behavior.
- case sensitivity.
- migrations.
- constraints.
- indexes/unique keys.
- SQL translation differences.

### Repository integration tests

Test:

- hero search filters.
- hero slug lookup.
- artifact options query.
- news article lookup.
- guide/floor/boss retrieval.
- team builder share save/load.
- gear optimization cache save/load.

Example:

```text
Given heroes exist
When searching by faction and class
Then only matching heroes are returned in expected order
```

### API integration tests

Test endpoints through the real ASP.NET Core pipeline.

Minimum API tests:

```text
GET /health/live returns 200
GET /health/ready returns 200 when database is available

GET /api/v1/heroes returns 200
GET /api/v1/heroes/{knownSlug} returns 200
GET /api/v1/heroes/{missingSlug} returns 404

GET /api/v1/news returns 200
GET /api/v1/news/{missingSlug} returns 404

GET /api/v1/tools/bp-deficit validates bad input
POST /api/v1/tools/gear-optimization validates bad input
POST /api/v1/team-builder/shares saves valid payload
GET /api/v1/team-builder/shares/{id} retrieves saved payload
```

## Migration tests

CI should verify that migrations apply cleanly to a blank PostgreSQL database.

Test:

```text
blank database
  -> apply all migrations
  -> optional seed/import
  -> readiness succeeds
```

Also test that the EF model has no uncommitted migration changes.

## Frontend unit/component tests

### Tools

- Angular test runner
- Vitest if already used by Angular setup
- Angular Testing Library optional

### What to test

- API services build URLs correctly.
- API services handle expected responses.
- Tool validation logic.
- Team builder serialization/deserialization.
- Filter state behavior.
- Reusable UI components with meaningful conditional behavior.

Avoid over-testing static templates.

## End-to-end tests

### Tool

Use Playwright.

### Purpose

E2E tests should prove the main deployed app works from a user perspective.

Minimum smoke tests:

```text
home page loads
heroes list loads
hero detail route loads
news article route loads
guide route loads
BP deficit calculator produces expected result
gear optimization form handles sample request
team builder can create and load a share link
```

### Test environments

Run E2E against:

- local app in CI where possible, and/or
- staging environment after deploy.

Production E2E should be limited to safe read-only smoke tests.

Do not create lots of production data from tests.

## Contract testing

Since the frontend and backend live in one repo, full contract testing can wait.

For now:

- Keep DTO TypeScript interfaces aligned with backend DTOs.
- Consider generating TypeScript clients from OpenAPI later.
- Add compile-time checks in frontend.
- Add API integration tests for response shapes.

Future option:

```text
OpenAPI generation -> TypeScript API client
```

This reduces drift between frontend and backend.

## Coverage expectations

Coverage is a signal, not the goal.

Suggested initial targets:

```text
Domain logic:
  high coverage, especially calculators/solvers

Application handlers:
  moderate coverage

Infrastructure:
  targeted integration coverage

API:
  key endpoint coverage

Frontend:
  critical tools/components only

E2E:
  smoke coverage
```

Do not block progress with arbitrary global coverage thresholds early.

Later, add thresholds for the most important projects.

## Test data strategy

Use explicit test builders/factories.

Examples:

```text
HeroBuilder
ArtifactBuilder
NewsPostBuilder
TeamBuilderPayloadFactory
GearOptimizationSampleInputs
```

Avoid copying huge test object literals everywhere.

Test data should be:

- small
- readable
- relevant to the test
- isolated between tests

## Snapshot tests

Snapshot tests can be useful for:

- gear optimization result structures
- content import output
- generated API examples

Rules:

- Use snapshots sparingly.
- Snapshot files must be reviewed like source code.
- Avoid huge snapshots that nobody reads.
- Prefer targeted assertions for critical logic.

## Performance and benchmark tests

Gear optimization may need benchmarks.

Use a separate benchmark project for:

- solver performance
- cache effectiveness
- worst-case inputs
- allocation behavior

Benchmarks do not need to run on every PR.

Run manually or in scheduled workflow.

## CI test stages

Pull request checks:

```text
backend build
backend unit tests
backend integration tests
frontend build
frontend tests
lint/format checks
docker build
```

Post-deploy staging checks:

```text
health check
database readiness check
Playwright smoke tests
```

Production checks:

```text
health check
read-only smoke test
```

## Test naming convention

Use descriptive names.

Recommended style:

```csharp
[Fact]
public async Task GetBySlug_ReturnsHero_WhenSlugExists()
```

or:

```csharp
[Fact]
public async Task HandleAsync_ReturnsNotFound_WhenShareCodeDoesNotExist()
```

Test names should explain:

```text
method/action
expected result
condition
```

## Testing quality checklist

- [ ] Domain logic has unit tests.
- [ ] Gear optimization has edge-case tests.
- [ ] Application handlers have tests with fake repositories.
- [ ] Repositories have PostgreSQL integration tests.
- [ ] API endpoints have integration tests.
- [ ] Migrations apply cleanly in CI.
- [ ] Frontend tools have tests.
- [ ] Team builder serialization has tests.
- [ ] Playwright smoke tests cover critical paths.
- [ ] Tests run in GitHub Actions.
- [ ] Failing tests block merges.
- [ ] Test data is isolated and repeatable.
