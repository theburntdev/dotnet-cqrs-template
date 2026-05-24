# PRD: Backend CQRS Scaffold

## Problem Statement

Starting a new .NET Web API project requires repeatedly wiring up the same infrastructure: CQRS plumbing, repository pattern, validation pipeline, error handling, and test scaffolding. Developers waste time on boilerplate and often make inconsistent architectural decisions. There is no opinionated, working starting point that demonstrates all these patterns end-to-end.

## Solution

A cloneable .NET solution that demonstrates a complete, opinionated CQRS architecture using ASP.NET Core minimal APIs, MediatR, Entity Framework Core (Postgres), FluentValidation, Mapperly, and Serilog. The solution includes a working `TodoItem` domain slice — fully tested, runnable out of the box — that developers can use as a reference before deleting and replacing with their own domain.

## User Stories

1. As a developer, I want to clone the repo and run `docker compose up -d` followed by `dotnet watch`, so that I have a working API without any manual setup.
2. As a developer, I want the README to explain local setup (Docker, user-secrets, hot reload), so that I can be productive in under 5 minutes.
3. As a developer, I want to see a complete CQRS slice for `TodoItem` (create, read, update, delete, list), so that I can understand how all layers connect.
4. As a developer, I want domain entities created via static factory methods, so that I cannot construct an invalid entity.
5. As a developer, I want status transitions handled by a domain method on the entity, so that business rules cannot be bypassed by callers.
6. As a developer, I want strongly-typed entity IDs (`TodoItemId`), so that I cannot accidentally pass the wrong ID type to a repository method.
7. As a developer, I want all expected domain errors returned as `Result<T>` rather than thrown exceptions, so that error paths are explicit in handler signatures.
8. As a developer, I want a non-generic `Result` type for void commands, so that delete/update handlers do not depend on MediatR's `Unit`.
9. As a developer, I want field-length constraints defined as constants on the entity, so that EF Core config and FluentValidation validators stay in sync automatically.
10. As a developer, I want validation errors to return `422 Unprocessable Entity` with Problem Details, so that API consumers receive structured, actionable error responses.
11. As a developer, I want unhandled exceptions to return `500 Internal Server Error` with Problem Details, so that API consumers never receive raw exception output.
12. As a developer, I want endpoints to unwrap `Result<T>` via a `ToHttpResult()` extension, so that inline `IsSuccess` branching never appears in endpoint bodies.
13. As a developer, I want query handlers to return a read model DTO directly, so that the endpoint forwards it unchanged with no mapping logic.
14. As a developer, I want command handlers to return `Result<TEntity>`, so that the endpoint in the Api layer owns the mapping to a response DTO.
15. As a developer, I want a single shared `TodoItemResponse` record in the Api layer used by both create and update endpoints, so that response shapes are consistent without duplication.
16. As a developer, I want a separate `TodoItemDto` read model in the Application layer, so that query responses can evolve independently from command response contracts.
17. As a developer, I want Mapperly mappers injected via interface, so that endpoints and handlers are testable without real mapping logic.
18. As a developer, I want `PATCH /api/todo-items/{id}` to treat `null` fields as "no change" and empty string `""` as "clear the value", so that optional fields can be cleared without ambiguous semantics.
19. As a developer, I want `GET /api/todo-items` to support an optional `?status=` query parameter, so that consumers can filter by status without loading all items.
20. As a developer, I want all collection endpoints to return a paged envelope `{ items, total, page, pageSize }`, so that clients can implement pagination consistently.
21. As a developer, I want `SaveChangesAsync` called explicitly via `IUnitOfWork` in each command handler, so that the transaction boundary is visible and intentional.
22. As a developer, I want each layer to register its own services via an `IServiceCollection` extension method, so that `Program.cs` stays clean and each layer owns its DI setup.
23. As a developer, I want a `LoggingBehavior` MediatR pipeline behavior that logs request type and elapsed milliseconds, so that I have basic observability without touching handler code.
24. As a developer, I want a `ValidationBehavior` MediatR pipeline behavior that runs FluentValidation before the handler, so that invalid requests never reach handler logic.
25. As a developer, I want Swagger UI available in development at the default path, so that I can explore and test the API interactively.
26. As a developer, I want a health check endpoint at `GET /health` that verifies the database connection, so that container orchestrators can probe liveness.
27. As a developer, I want EF Core configured with snake_case column/table names, so that the Postgres schema follows Postgres naming conventions while C# stays PascalCase.
28. As a developer, I want strongly-typed IDs automatically converted by a global EF Core convention, so that adding a new entity requires no per-entity value converter boilerplate.
29. As a developer, I want an initial EF Core migration included, so that `dotnet ef database update` produces a working schema immediately after cloning.
30. As a developer, I want integration tests using Testcontainers with `MigrateAsync`, so that tests run against the real schema and catch migration bugs.
31. As a developer, I want unit tests for domain entity behavior, so that status transition logic and factory methods are verified in isolation.
32. As a developer, I want unit tests for command/query handlers using NSubstitute mocks, so that application logic is verified without a real database.
33. As a developer, I want `TodoItemBuilder` in `BackendTemplate.Testing.Common` with recipes, so that test data construction is consistent and valid by default.

## Implementation Decisions

### Solution Structure
Five production projects + four test projects + one shared test helper:
- `BackendTemplate.Domain` — entities, value objects, `Result<T>`, `TodoStatus`, strongly-typed IDs. Zero framework references.
- `BackendTemplate.Application` — MediatR commands/queries/handlers, FluentValidation validators, repo interfaces, `IUnitOfWork`, `Page<T>`, `TodoItemDto`.
- `BackendTemplate.Infrastructure` — `AppDbContext`, EF entity configurations, repository implementations, `UnitOfWork`.
- `BackendTemplate.Api` — minimal API endpoints, `TodoItemResponse`, Mapperly mappers, global exception handler, DI wiring in `Program.cs`.
- `BackendTemplate.Testing.Common` — `TodoItemBuilder` with recipes, shared fixtures.
- Test projects mirror production projects (Api.Tests deferred).

### Dependency Direction
```
Api → Application → Domain
Infrastructure → Application
```
`Domain` has zero references to any other solution project.

### Domain: TodoItem
Factory method: `TodoItem.Create(string title, string? description)` sets `Id = new TodoItemId(Guid.NewGuid())`, `Status = Pending`, `CreatedAtUtc = DateTime.UtcNow`. Private parameterless constructor exists for EF Core materialization only.

Domain method: `TodoItem.UpdateStatus(TodoStatus newStatus)` sets `CompletedAtUtc = DateTime.UtcNow` when transitioning to `Done`; clears it otherwise.

Field constraints as constants on the entity:
- `public const int TitleMaxLength = 200`
- `public const int DescriptionMaxLength = 2000`

Both EF Core configuration and FluentValidation validators reference these constants.

### Domain: Result<T> and Result
Custom implementations in `BackendTemplate.Domain.Common`:
```
Result<T>.Success(T value)
Result<T>.Failure(string error, ErrorKind kind = ErrorKind.Validation)
Result.Success()
Result.Failure(string error, ErrorKind kind = ErrorKind.Validation)
enum ErrorKind { Validation, NotFound, Conflict }
```
`ToHttpResult()` extension in `BackendTemplate.Api` maps: `Validation → 422`, `NotFound → 404`, `Conflict → 409`.

### CQRS Operations
| Operation | Type | Endpoint |
|---|---|---|
| CreateTodoItem | Command → `Result<TodoItem>` | `POST /api/todo-items` |
| GetTodoItem | Query → `TodoItemDto` | `GET /api/todo-items/{id}` |
| GetTodoItems | Query → `Page<TodoItemDto>` | `GET /api/todo-items?page&pageSize&status` |
| UpdateTodoItem | Command → `Result<TodoItem>` | `PATCH /api/todo-items/{id}` |
| DeleteTodoItem | Command → `Result` | `DELETE /api/todo-items/{id}` |

### Update Semantics (PATCH)
`UpdateTodoItemCommand` uses nullable fields. `null` = skip update. `""` (empty string) = clear optional field. This is a documented limitation — clients cannot use `null` to clear `Description`; they must send `""`.

### Mapper Placement
- `ITodoItemMapper` interface + Mapperly implementation in `BackendTemplate.Api` — maps `TodoItem → TodoItemResponse`.
- Query handlers in `BackendTemplate.Application` map `TodoItem → TodoItemDto` directly (no separate mapper type needed).

### DI Registration
- `BackendTemplate.Application`: `AddApplication(IServiceCollection)` — MediatR, pipeline behaviors (Logging then Validation), FluentValidation validators via assembly scan.
- `BackendTemplate.Infrastructure`: `AddInfrastructure(IServiceCollection, IConfiguration)` — `AppDbContext`, repositories, `IUnitOfWork`.
- `BackendTemplate.Api` `Program.cs`: calls both, registers mappers, configures Serilog, adds health checks, maps endpoints.

### MediatR Pipeline Order (outermost first)
1. `LoggingBehavior<TRequest, TResponse>` — logs request type name + elapsed ms. Never logs request/response body.
2. `ValidationBehavior<TRequest, TResponse>` — runs all `IValidator<TRequest>` validators; throws `ValidationException` on failure.

### EF Core
- `AppDbContext` in Infrastructure.
- `UseSnakeCaseNamingConvention()` (EFCore.NamingConventions package) — all table/column names snake_case in Postgres; C# properties remain PascalCase.
- Strongly-typed ID value converters registered globally via `AppDbContext.ConfigureConventions` — no per-entity converter needed.
- One `IEntityTypeConfiguration<TodoItem>` class (`TodoItemConfiguration`) in Infrastructure.
- Initial migration committed to the repository.

### Routing
- All todo-item routes grouped under `/api/todo-items` via `MapGroup`.
- `MapTodoItemEndpoints(WebApplication app)` extension method in `BackendTemplate.Api/TodoItems/TodoItemEndpoints.cs`.

### Observability
- Serilog with `WriteTo.Console()`, `FromLogContext()`, `WithEnvironmentName()`, `WithMachineName()`.
- Configured only in `BackendTemplate.Api` host; all other projects use `ILogger<T>`.

### Developer Experience
- `docker-compose.yml` at repo root with Postgres service.
- README documents: `docker compose up -d`, `dotnet user-secrets set`, `dotnet watch`, `dotnet test`.
- Swagger UI (Swashbuckle) served in development only at default path.
- Health check at `GET /health` via `AddHealthChecks().AddDbContextCheck<AppDbContext>()`.

## Testing Decisions

Good tests verify observable behavior, not implementation details. Prefer testing via public interfaces; avoid asserting on private state or mocking types you own.

### Domain Tests (`BackendTemplate.Domain.Tests`)
- Unit tests for `TodoItem` entity: factory method sets correct defaults, `UpdateStatus` transitions set/clear `CompletedAtUtc`, `Result<T>` success/failure behavior.
- No database, no mocks — plain xUnit, no fixtures.
- Naming: `{MethodUnderTest}_Given{Scenario}_Then{Assertion}`.

### Application Tests (`BackendTemplate.Application.Tests`)
- Unit tests for each command/query handler using NSubstitute strict mocks for repositories and `IUnitOfWork`.
- Verify handler calls `SaveChangesAsync` after mutations.
- Verify handler returns correct `Result` on not-found, conflict, and success paths.
- Naming: `Handle_Given{Scenario}_Then{Assertion}`.

### Infrastructure Tests (`BackendTemplate.Infrastructure.Tests`)
- Integration tests for repository methods using Testcontainers Postgres via `IClassFixture<DatabaseFixture>`.
- `DatabaseFixture` spins up a container, applies migrations via `MigrateAsync()`, exposes a scoped `AppDbContext`.
- Each test wraps in a transaction rolled back in `Dispose` — zero data leakage.
- Tests use `TodoItemBuilder` from `BackendTemplate.Testing.Common`.

### Test Data
- `TodoItemBuilder` in `BackendTemplate.Testing.Common` with fluent `WithX()` methods.
- `Build()` produces a valid entity. Default values satisfy all invariants.
- Recipes: `TodoItemBuilder.InProgress()`, `TodoItemBuilder.Done()`.
- Never construct domain entities directly in tests.

## Out of Scope

- Authentication and authorization (explicitly deferred — no JWT, no auth middleware, no `[Authorize]`).
- `BackendTemplate.Api.Tests` full-stack integration tests (deferred to a future task).
- Frontend / client application.
- Deployment configuration (CI/CD, Kubernetes, cloud infrastructure).
- Event sourcing, outbox pattern, or domain events.
- Multi-tenancy.
- Caching.
- Rate limiting.
- API versioning (route prefix `/api` only; no version segment).

## Further Notes

- The `TodoItem` slice is a permanent, working example — not a throwaway scaffold. Developers clone the repo, verify `dotnet test` is green, then delete the `TodoItems` slice and replace it with their own domain.
- `page` is 1-based at the API layer. Repository implementations translate to 0-based EF `Skip()` via `(page - 1) * pageSize`.
- `IResult` (ASP.NET Core) is an endpoint concern only — never used in Application or Domain.
- `CancellationToken ct = default` is the last parameter on every handler method and every repository interface method.
- Serilog is referenced only in `BackendTemplate.Api` — all other projects inject `ILogger<T>` only.
