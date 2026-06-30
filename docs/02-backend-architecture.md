# Backend Architecture

## Purpose

This document defines the intended backend architecture for SilverHub.

The backend should be organized like a production-style ASP.NET Core application, not a simple controller-to-database demo. The goal is to keep the codebase understandable, testable, and explainable in interviews.

## Technology choices

- Language: C#
- Runtime/framework: ASP.NET Core
- Database access: Entity Framework Core
- Database provider: PostgreSQL via Npgsql
- API style: REST-ish JSON API
- API documentation: OpenAPI/Swagger
- Validation: FluentValidation
- Logging: built-in `Microsoft.Extensions.Logging` with JSON console formatter (`AddJsonConsole`)
- Error responses: ProblemDetails
- Tests: xUnit, integration tests, and API smoke tests

## Project structure

Recommended backend project layout:

```text
src/
  SilverHub.Api/
  SilverHub.Application/
  SilverHub.Domain/
  SilverHub.Infrastructure/
tests/
  SilverHub.Domain.Tests/
  SilverHub.Application.Tests/
  SilverHub.Api.Tests/
  SilverHub.Infrastructure.Tests/
```

Optional supporting projects:

```text
tools/
  SilverHub.DbMaintenance/
  SilverHub.ContentImport/
  SilverHub.GearOptimizationBench/
```

## Layer responsibilities

### SilverHub.Api

The API project is the HTTP boundary.

Responsibilities:

- Define controllers/endpoints.
- Accept route/query/body input.
- Return HTTP responses.
- Apply authentication/authorization if added.
- Apply API-specific filters/middleware.
- Configure CORS.
- Configure Swagger/OpenAPI.
- Configure health checks.
- Configure exception handling.
- Configure request logging.
- Wire dependency injection.

The API project should not contain business logic or EF Core query logic.

Controllers should call application handlers.

Acceptable controller responsibilities:

- Basic HTTP route binding.
- Calling a handler.
- Translating handler result to HTTP status code.
- Returning `Ok`, `NotFound`, `Created`, `NoContent`, etc.

Avoid:

- Direct `DbContext` usage in controllers.
- Complex LINQ queries in controllers.
- Game calculation logic in controllers.
- Manual JSON string handling.
- Inconsistent validation.

### SilverHub.Application

The application layer contains use cases.

Responsibilities:

- Define commands and queries.
- Define handlers.
- Define DTOs returned to the API/frontend.
- Define ports/interfaces required by use cases.
- Coordinate domain logic.
- Apply application-level validation.
- Return predictable results.

Examples:

```text
GetHeroesListQuery
GetHeroesListHandler
GetHeroBySlugQuery
GetHeroBySlugHandler
CalculateBpDeficitQuery
CalculateBpDeficitHandler
CalculateGearOptimizationCommand
CalculateGearOptimizationHandler
CreateTeamBuilderShareCommand
CreateTeamBuilderShareHandler
```

Application layer should depend on:

```text
SilverHub.Domain
```

Application layer should not depend on:

```text
SilverHub.Infrastructure
Entity Framework Core
ASP.NET Core MVC
PostgreSQL provider
```

This keeps application use cases testable without a real API or database.

### SilverHub.Domain

The domain layer contains core business/game concepts and pure logic.

Responsibilities:

- Entities.
- Value objects.
- Domain services.
- Domain exceptions where useful.
- Core calculations that do not depend on infrastructure.
- Validation rules that are true regardless of API/database.

Examples:

```text
Hero
Artifact
NewsPost
TeamBuilderShare
GearOptimizationSolver
BpDeficitCalculator
Slug
Rarity
Faction
```

Domain layer should avoid dependencies on frameworks.

Domain layer should not depend on:

```text
ASP.NET Core
Entity Framework Core
Npgsql
Cloud/AWS SDKs
```

### SilverHub.Infrastructure

The infrastructure layer implements external details.

Responsibilities:

- EF Core `DbContext`.
- EF Core entity configurations.
- Repository implementations.
- Database migrations.
- SQL/PostgreSQL-specific behavior.
- File/object storage adapters if backend directly manages assets.
- Cache implementations.
- External service clients.

Infrastructure depends on Application and Domain because it implements application ports.

Examples:

```text
HeroRepository : IHeroRepository
ArtifactRepository : IArtifactRepository
NewsPostRepository : INewsPostRepository
TeamBuilderShareRepository : ITeamBuilderShareRepository
GearOptimizationCacheRepository : IGearOptimizationCacheRepository
```

## Dependency rule

Dependencies should flow inward.

```text
Api -> Application -> Domain
Api -> Infrastructure -> Application -> Domain
Infrastructure -> Domain
```

Forbidden dependency examples:

```text
Domain -> Application
Domain -> Infrastructure
Application -> Infrastructure
Application -> Api
Infrastructure -> Api
```

## Request flow

Typical read request:

```text
HTTP GET /api/v1/heroes
  -> HeroesController
  -> GetHeroesListHandler
  -> IHeroRepository
  -> HeroRepository
  -> SilverHubDbContext
  -> PostgreSQL
  -> HeroListItemDto[]
  -> 200 OK
```

Typical command/write request:

```text
HTTP POST /api/v1/team-builder/shares
  -> TeamBuilderSharesController
  -> validation
  -> CreateTeamBuilderShareHandler
  -> TeamBuilderShare domain/model
  -> ITeamBuilderShareRepository
  -> PostgreSQL
  -> generated share id
  -> 201 Created or 200 OK
```

## API design conventions

Base route:

```text
/api/v1
```

Resource examples:

```text
GET  /api/v1/heroes
GET  /api/v1/heroes/{slug}
GET  /api/v1/heroes/options

GET  /api/v1/news
GET  /api/v1/news/{slug}

GET  /api/v1/artifacts
GET  /api/v1/artifacts/options

GET  /api/v1/guides/clan-hunt/seasons
GET  /api/v1/guides/clan-hunt/seasons/{seasonSlug}

GET  /api/v1/tools/bp-deficit
POST /api/v1/tools/gear-optimization

POST /api/v1/team-builder/shares
GET  /api/v1/team-builder/shares/{id}
```

Prefer stable API URLs even if frontend route names change.

## DTO strategy

Do not expose EF Core entities directly through API responses.

Use DTOs for all API responses and request bodies.

Reasons:

- Keeps API contracts stable.
- Prevents accidental data exposure.
- Avoids serialization cycles.
- Allows database schema to evolve independently.
- Allows frontend to receive exactly the shape it needs.

Naming conventions:

```text
HeroListItemDto
HeroDetailDto
HeroOptionDto
GearOptimizationRequestDto
GearOptimizationResultDto
TeamBuilderSharePayloadDto
```

DTOs should be in Application unless they are purely API-specific.

## Commands, queries, and handlers

Use simple command/query handler classes.

Example pattern:

```csharp
public sealed record GetHeroBySlugQuery(string Slug);

public sealed class GetHeroBySlugHandler
{
    private readonly IHeroRepository _heroes;

    public GetHeroBySlugHandler(IHeroRepository heroes)
    {
        _heroes = heroes;
    }

    public Task<HeroDetailDto?> HandleAsync(GetHeroBySlugQuery query, CancellationToken ct)
    {
        return _heroes.GetBySlugAsync(query.Slug, ct);
    }
}
```

Guidelines:

- One handler per use case.
- Handler methods should accept `CancellationToken`.
- Handlers should return DTOs, result objects, or domain results.
- Handlers should not know about HTTP.
- Handlers should not return `IActionResult`.

## Repository strategy

Repositories should exist where they clarify use cases and isolate persistence details.

Do not create generic repositories such as:

```text
IRepository<T>
```

Prefer specific repositories:

```text
IHeroRepository
INewsPostRepository
IArtifactRepository
ITeamBuilderShareRepository
IGearOptimizationCacheRepository
```

Repository methods should express use-case needs:

```csharp
Task<IReadOnlyList<HeroListItemDto>> SearchAsync(HeroListQuery query, CancellationToken ct);
Task<HeroDetailDto?> GetBySlugAsync(string slug, CancellationToken ct);
```

It is acceptable for repository methods to return DTO projections for read-heavy query scenarios.

## Validation strategy

Validation should be consistent.

Approach:

- Use **FluentValidation** for all request DTO validation.
- Use domain validation for rules that belong to the domain.
- Use centralized validation response formatting.
- Return ProblemDetails-compatible errors.

Validator classes live in `SilverHub.Application` next to the DTO they validate. Validators are auto-registered via assembly scanning. Validation runs through ASP.NET Core's model binding pipeline so failures produce `400 Bad Request` with ProblemDetails before reaching handlers.

Validation location:

```text
API boundary:
  request shape, required fields, basic ranges

Application:
  use-case rules

Domain:
  invariant rules that must always hold
```

Avoid spreading manual `if` checks across controllers unless the validation is genuinely HTTP-specific.

## Error handling

Use a global exception handling strategy.

Recommended responses:

```text
400 Bad Request
  validation errors

404 Not Found
  missing resource

409 Conflict
  duplicate or invalid state transition

422 Unprocessable Entity
  semantically invalid request, if needed

500 Internal Server Error
  unexpected exceptions
```

Use ProblemDetails for errors.

Example:

```json
{
  "type": "https://silverhub.app/errors/validation",
  "title": "Validation failed",
  "status": 400,
  "traceId": "00-...",
  "errors": {
    "enemyBp": ["enemyBp must be greater than 0."]
  }
}
```

## Logging strategy

Backend code should use structured logging.

Minimum required logs:

- Request start/end with method, path, status code, elapsed time, trace id.
- Unhandled exceptions with trace id.
- Slow requests above threshold.
- Expensive tool calculations.
- Deployment/build version at startup.
- Database migration status.

Do not log:

- Full request bodies by default.
- Secrets.
- Database passwords.
- Authorization headers.
- User-submitted notes unless explicitly required for debugging and redacted.

## Health checks

Expose:

```text
GET /health/live
GET /health/ready
```

Definitions:

```text
/health/live
  Process is running.

/health/ready
  Process is ready to serve traffic and can reach required dependencies.
```

Readiness should check:

- Database connectivity.
- Required configuration values.
- Optional cache/storage dependencies if introduced.

## API versioning

Start with route versioning:

```text
/api/v1
```

Do not add a heavy versioning library unless needed.

Breaking API changes should create `/api/v2`.

## Background jobs

Avoid background jobs at first unless needed.

Potential future jobs:

- Refresh derived guide/content data.
- Rebuild search indexes.
- Prune old team builder shares.
- Warm gear optimization cache.
- Import content from structured files.

If needed, prefer:

- Small scheduled GitHub Action for maintenance tasks, or
- AWS EventBridge Scheduler invoking a maintenance command/container.

## Performance principles

Backend performance expectations:

- API endpoints should avoid loading large object graphs unnecessarily.
- Use DTO projections in EF Core queries.
- Add indexes for slug and frequent filters.
- Paginate list endpoints.
- Cache expensive gear optimization results.
- Add request timeouts/limits for heavy calculations.
- Avoid synchronous blocking on async operations.

## OpenAPI/Swagger

Swagger is enabled in **Development and Staging**. **Disabled in Production.**

The Swagger middleware is registered only when `env.IsDevelopment()` or the environment name is `Staging`. Production builds skip Swagger registration entirely so the endpoint does not exist at all (not merely 401'd).

Swagger should include:

- Route summaries.
- Response types.
- Error response examples.
- DTO schemas.

## Coding conventions

- Enable nullable reference types.
- Use `sealed` classes where inheritance is not intended.
- Prefer records for immutable DTOs.
- Prefer async all the way for I/O.
- Pass `CancellationToken`.
- Avoid static mutable state.
- Keep controllers thin.
- Keep methods small enough to test.
- Prefer explicit names over clever abstractions.

## Backend quality checklist

- [ ] Controllers do not use `DbContext` directly.
- [ ] Use cases are represented by handlers.
- [ ] Application layer does not depend on Infrastructure.
- [ ] DTOs are used for request/response contracts.
- [ ] Error responses are consistent.
- [ ] Request validation is consistent.
- [ ] Health checks exist.
- [ ] Structured logging exists.
- [ ] API has integration tests.
- [ ] Domain logic has unit tests.
- [ ] EF migrations are committed.
- [ ] Secrets are not committed.
- [ ] Dockerfile supports production deployment.
- [ ] API can be run locally with Docker Compose PostgreSQL.
