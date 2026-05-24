# API Layer (Plan 4) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `BackendTemplate.Api` — an ASP.NET Core minimal API host that wires together the Application and Infrastructure layers with 5 TodoItem endpoints, Mapperly mapping, Serilog logging, OpenAPI/Scalar UI, and a health check.

**Architecture:** Thin API layer that dispatches to MediatR, maps `Result<TodoItem>` → `TodoItemResponse` via Mapperly, and returns TypedResults. Query handlers return `Page<TodoItemDto>` (already a DTO) forwarded unchanged; command handlers return `Result<TodoItem>` which the endpoint maps to `TodoItemResponse`.

**Tech Stack:** ASP.NET Core minimal APIs, MediatR (ISender), Mapperly 3.6 (source-generated), Serilog 8, FluentValidation (pipeline behavior), Scalar.AspNetCore, Microsoft.AspNetCore.OpenApi

---

## Key Facts (read before coding)

- `Page<T>` record (Application layer) uses property `PageNumber` (not `Page` — avoids CS0542). The `PagedResponse<T>` API model exposes `Page` for correct JSON output.
- `TodoItemId` is `record struct TodoItemId(Guid Value)` — Mapperly needs a static converter method.
- `CreateTodoItemCommand(string Title, string? Description)` is bound directly from JSON body.
- `UpdateTodoItemRequest` carries `Title?`, `Description?`, `Status?`; `Id` always comes from the route.
- PATCH semantics: `null` = skip field, `""` = clear Description (maps to null in handler).
- `Result<T>` / `Result`: `IsSuccess`, `.Value`, `.Error`, `.Kind` (ErrorKind enum: Validation, NotFound, Conflict).
- `GlobalExceptionHandler` handles `ValidationException` → 422, else → 500 Problem Details.
- Serilog configured in code (not `ReadFrom.Configuration`). No Serilog references outside Api project.
- Never commit `appsettings.Development.json` with real connection string — use `dotnet user-secrets`.
- Connection string key: `ConnectionStrings:Default`.
- `Directory.Build.props` already sets `TargetFramework`, `Nullable`, `ImplicitUsings`, `LangVersion` — do NOT repeat in Api.csproj.
- No Api.Tests project in this plan (deferred).

---

## File Map

| Action | Path |
|--------|------|
| Create | `backend/src/BackendTemplate.Api/BackendTemplate.Api.csproj` |
| Create | `backend/src/BackendTemplate.Api/appsettings.json` |
| Create | `backend/src/BackendTemplate.Api/appsettings.Development.json` |
| Create | `backend/src/BackendTemplate.Api/Program.cs` |
| Create | `backend/src/BackendTemplate.Api/Models/TodoItemResponse.cs` |
| Create | `backend/src/BackendTemplate.Api/Models/UpdateTodoItemRequest.cs` |
| Create | `backend/src/BackendTemplate.Api/Models/PagedResponse.cs` |
| Create | `backend/src/BackendTemplate.Api/Mappers/ITodoItemMapper.cs` |
| Create | `backend/src/BackendTemplate.Api/Mappers/TodoItemMapper.cs` |
| Create | `backend/src/BackendTemplate.Api/Extensions/ResultExtensions.cs` |
| Create | `backend/src/BackendTemplate.Api/Exceptions/GlobalExceptionHandler.cs` |
| Create | `backend/src/BackendTemplate.Api/Endpoints/TodoItemEndpoints.cs` |
| Create | `docker-compose.yml` (repo root) |
| Modify | `backend/BackendTemplate.slnx` |
| Modify | `README.md` |

---

## Task 1: Create Api Project

**Files:**
- Create: `backend/src/BackendTemplate.Api/BackendTemplate.Api.csproj`
- Create: `backend/src/BackendTemplate.Api/appsettings.json`
- Create: `backend/src/BackendTemplate.Api/appsettings.Development.json`
- Modify: `backend/BackendTemplate.slnx`

- [ ] **Step 1: Create BackendTemplate.Api.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.5" />
    <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="9.0.5" />
    <PackageReference Include="Riok.Mapperly" Version="3.6.0" />
    <PackageReference Include="Scalar.AspNetCore" Version="2.0.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
    <PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../BackendTemplate.Application/BackendTemplate.Application.csproj" />
    <ProjectReference Include="../BackendTemplate.Infrastructure/BackendTemplate.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Create appsettings.json**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 3: Create appsettings.Development.json** (no connection strings — use user-secrets)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

- [ ] **Step 4: Add project to backend/BackendTemplate.slnx**

Add `<Project Path="src/BackendTemplate.Api/BackendTemplate.Api.csproj" />` inside the `<Folder Name="/src/">` element:

```xml
<Solution>
  <Folder Name="/src/">
    <Project Path="src/BackendTemplate.Api/BackendTemplate.Api.csproj" />
    <Project Path="src/BackendTemplate.Application/BackendTemplate.Application.csproj" />
    <Project Path="src/BackendTemplate.Domain/BackendTemplate.Domain.csproj" />
    <Project Path="src/BackendTemplate.Infrastructure/BackendTemplate.Infrastructure.csproj" />
  </Folder>
  <Folder Name="/tests/">
    <Project Path="tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj" />
    <Project Path="tests/BackendTemplate.Domain.Tests/BackendTemplate.Domain.Tests.csproj" />
    <Project Path="tests/BackendTemplate.Infrastructure.Tests/BackendTemplate.Infrastructure.Tests.csproj" />
    <Project Path="tests/BackendTemplate.Testing.Common/BackendTemplate.Testing.Common.csproj" />
  </Folder>
</Solution>
```

- [ ] **Step 5: Restore packages**

Run: `dotnet restore backend/BackendTemplate.slnx`
Expected: Succeeds with no errors.

- [ ] **Step 6: Commit**

```bash
git add backend/src/BackendTemplate.Api/BackendTemplate.Api.csproj
git add backend/src/BackendTemplate.Api/appsettings.json
git add backend/src/BackendTemplate.Api/appsettings.Development.json
git add backend/BackendTemplate.slnx
git commit -m "feat: add BackendTemplate.Api project skeleton"
```

---

## Task 2: Add API Models

**Files:**
- Create: `backend/src/BackendTemplate.Api/Models/TodoItemResponse.cs`
- Create: `backend/src/BackendTemplate.Api/Models/UpdateTodoItemRequest.cs`
- Create: `backend/src/BackendTemplate.Api/Models/PagedResponse.cs`

- [ ] **Step 1: Create TodoItemResponse.cs**

```csharp
namespace BackendTemplate.Api.Models;

using BackendTemplate.Domain.TodoItems;

public record TodoItemResponse(
    Guid Id,
    string Title,
    string? Description,
    TodoStatus Status,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
```

- [ ] **Step 2: Create UpdateTodoItemRequest.cs**

```csharp
namespace BackendTemplate.Api.Models;

using BackendTemplate.Domain.TodoItems;

public record UpdateTodoItemRequest(
    string? Title,
    string? Description,
    TodoStatus? Status);
```

- [ ] **Step 3: Create PagedResponse.cs**

Note: `Page<T>` uses `PageNumber` internally (CS0542 fix). This wrapper exposes `Page` for the JSON response to match the API contract `{ "items": [...], "total": n, "page": n, "pageSize": n }`.

```csharp
namespace BackendTemplate.Api.Models;

using BackendTemplate.Application.Common;

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize)
{
    public static PagedResponse<T> From(Page<T> page)
        => new(page.Items, page.Total, page.PageNumber, page.PageSize);
}
```

- [ ] **Step 4: Build**

Run: `dotnet build backend/BackendTemplate.slnx`
Expected: 0 errors (Api project compiles with just models).

- [ ] **Step 5: Commit**

```bash
git add backend/src/BackendTemplate.Api/Models/
git commit -m "feat: add TodoItemResponse, UpdateTodoItemRequest, PagedResponse models"
```

---

## Task 3: Add Mapperly Mapper

**Files:**
- Create: `backend/src/BackendTemplate.Api/Mappers/ITodoItemMapper.cs`
- Create: `backend/src/BackendTemplate.Api/Mappers/TodoItemMapper.cs`

- [ ] **Step 1: Create ITodoItemMapper.cs**

```csharp
namespace BackendTemplate.Api.Mappers;

using BackendTemplate.Api.Models;
using BackendTemplate.Domain.TodoItems;

public interface ITodoItemMapper
{
    TodoItemResponse Map(TodoItem item);
}
```

- [ ] **Step 2: Create TodoItemMapper.cs**

The private static `MapId` method tells Mapperly how to convert `TodoItemId` → `Guid`. Mapperly detects it by matching source/target types, not by name. The `partial` method declaration drives code generation.

```csharp
namespace BackendTemplate.Api.Mappers;

using BackendTemplate.Api.Models;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using Riok.Mapperly.Abstractions;

[Mapper]
public partial class TodoItemMapper : ITodoItemMapper
{
    public partial TodoItemResponse Map(TodoItem item);

    private static Guid MapId(TodoItemId id) => id.Value;
}
```

- [ ] **Step 3: Build**

Run: `dotnet build backend/BackendTemplate.slnx`
Expected: 0 errors. Mapperly generates the `Map` implementation at compile time — no runtime reflection.

- [ ] **Step 4: Commit**

```bash
git add backend/src/BackendTemplate.Api/Mappers/
git commit -m "feat: add ITodoItemMapper interface and Mapperly source-generated implementation"
```

---

## Task 4: Add ResultExtensions

**Files:**
- Create: `backend/src/BackendTemplate.Api/Extensions/ResultExtensions.cs`

- [ ] **Step 1: Create ResultExtensions.cs**

Maps `Result<T>` / `Result` to `IResult` (ASP.NET Core). Error kinds map to Problem Details: NotFound → 404, Conflict → 409, Validation → 422.

```csharp
namespace BackendTemplate.Api.Extensions;

using BackendTemplate.Domain.Common;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
        => result.IsSuccess
            ? onSuccess(result.Value)
            : MapFailure(result.Error, result.Kind);

    public static IResult ToHttpResult<T>(this Result<T> result)
        => result.ToHttpResult(value => TypedResults.Ok(value));

    public static IResult ToHttpResult(this Result result)
        => result.IsSuccess
            ? TypedResults.NoContent()
            : MapFailure(result.Error, result.Kind);

    private static IResult MapFailure(string error, ErrorKind kind)
        => kind switch
        {
            ErrorKind.NotFound => TypedResults.Problem(error, statusCode: 404),
            ErrorKind.Conflict => TypedResults.Problem(error, statusCode: 409),
            _ => TypedResults.Problem(error, statusCode: 422)
        };
}
```

- [ ] **Step 2: Build**

Run: `dotnet build backend/BackendTemplate.slnx`
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add backend/src/BackendTemplate.Api/Extensions/ResultExtensions.cs
git commit -m "feat: add ResultExtensions for mapping Result<T>/Result to IResult"
```

---

## Task 5: Add GlobalExceptionHandler

**Files:**
- Create: `backend/src/BackendTemplate.Api/Exceptions/GlobalExceptionHandler.cs`

- [ ] **Step 1: Create GlobalExceptionHandler.cs**

`ValidationException` from FluentValidation (thrown by `ValidationBehavior`) → 422 with field-level errors. All other exceptions → 500 with generic message (do not leak details).

```csharp
namespace BackendTemplate.Api.Exceptions;

using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(
                new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Validation failed"
                },
                cancellationToken);
            return true;
        }

        _logger.LogError(exception, "Unhandled exception");
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred"
            },
            cancellationToken);
        return true;
    }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build backend/BackendTemplate.slnx`
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add backend/src/BackendTemplate.Api/Exceptions/GlobalExceptionHandler.cs
git commit -m "feat: add GlobalExceptionHandler (ValidationException→422, unhandled→500)"
```

---

## Task 6: Add TodoItemEndpoints

**Files:**
- Create: `backend/src/BackendTemplate.Api/Endpoints/TodoItemEndpoints.cs`

- [ ] **Step 1: Create TodoItemEndpoints.cs**

5 endpoints via `MapGroup("/api/todo-items")`. Query results (`TodoItemDto`, `Page<TodoItemDto>`) forwarded unchanged. Command results (`Result<TodoItem>`) mapped to `TodoItemResponse` via `ITodoItemMapper`. Services (`ISender`, `ITodoItemMapper`, `CancellationToken`) are auto-resolved by the minimal API binder.

```csharp
namespace BackendTemplate.Api.Endpoints;

using BackendTemplate.Api.Extensions;
using BackendTemplate.Api.Mappers;
using BackendTemplate.Api.Models;
using BackendTemplate.Application.TodoItems.CreateTodoItem;
using BackendTemplate.Application.TodoItems.DeleteTodoItem;
using BackendTemplate.Application.TodoItems.GetTodoItem;
using BackendTemplate.Application.TodoItems.GetTodoItems;
using BackendTemplate.Application.TodoItems.UpdateTodoItem;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public static class TodoItemEndpoints
{
    public static IEndpointRouteBuilder MapTodoItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todo-items");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPatch("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);

        return app;
    }

    private static async Task<IResult> GetAll(
        ISender sender,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TodoStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetTodoItemsQuery(page, pageSize, status), ct);
        return TypedResults.Ok(PagedResponse<TodoItemDto>.From(result));
    }

    private static async Task<IResult> GetById(
        Guid id,
        ISender sender,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetTodoItemQuery(new TodoItemId(id)), ct);
        return result.ToHttpResult(dto => TypedResults.Ok(dto));
    }

    private static async Task<IResult> Create(
        [FromBody] CreateTodoItemCommand command,
        ISender sender,
        ITodoItemMapper mapper,
        CancellationToken ct = default)
    {
        var result = await sender.Send(command, ct);
        return result.ToHttpResult(item =>
            TypedResults.Created($"/api/todo-items/{item.Id.Value}", mapper.Map(item)));
    }

    private static async Task<IResult> Update(
        Guid id,
        [FromBody] UpdateTodoItemRequest request,
        ISender sender,
        ITodoItemMapper mapper,
        CancellationToken ct = default)
    {
        var command = new UpdateTodoItemCommand(
            new TodoItemId(id),
            request.Title,
            request.Description,
            request.Status);
        var result = await sender.Send(command, ct);
        return result.ToHttpResult(item => TypedResults.Ok(mapper.Map(item)));
    }

    private static async Task<IResult> Delete(
        Guid id,
        ISender sender,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new DeleteTodoItemCommand(new TodoItemId(id)), ct);
        return result.ToHttpResult();
    }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build backend/BackendTemplate.slnx`
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add backend/src/BackendTemplate.Api/Endpoints/TodoItemEndpoints.cs
git commit -m "feat: add TodoItemEndpoints (GET, POST, PATCH, DELETE)"
```

---

## Task 7: Wire Up Program.cs

**Files:**
- Create: `backend/src/BackendTemplate.Api/Program.cs`

- [ ] **Step 1: Create Program.cs**

Serilog bootstrap logger handles startup crashes. `UseSerilog` overrides after builder is configured. `AppDbContext` (from Infrastructure) used for the health check.

```csharp
using BackendTemplate.Api.Endpoints;
using BackendTemplate.Api.Exceptions;
using BackendTemplate.Api.Mappers;
using BackendTemplate.Application;
using BackendTemplate.Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) => config
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .WriteTo.Console());

    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' not found.");

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(connectionString);
    builder.Services.AddScoped<ITodoItemMapper, TodoItemMapper>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddOpenApi();
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>();

    var app = builder.Build();

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.MapHealthChecks("/health");
    app.MapTodoItemEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

- [ ] **Step 2: Build**

Run: `dotnet build backend/BackendTemplate.slnx`
Expected: 0 errors, 0 warnings (or only minor nullable warnings).

- [ ] **Step 3: Initialize user secrets**

Run from repo root:
```
dotnet user-secrets init --project backend/src/BackendTemplate.Api
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=backendtemplate;Username=postgres;Password=postgres" --project backend/src/BackendTemplate.Api
```

This stores the connection string in `%APPDATA%\Microsoft\UserSecrets\` — never in the repo.

- [ ] **Step 4: Commit** (do NOT commit appsettings.Development.json with real values)

```bash
git add backend/src/BackendTemplate.Api/Program.cs
git commit -m "feat: wire up Program.cs with Serilog, DI, OpenAPI, health check"
```

---

## Task 8: Add Docker Compose and Update README

**Files:**
- Create: `docker-compose.yml` (repo root)
- Modify: `README.md`

- [ ] **Step 1: Create docker-compose.yml at repo root**

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: backendtemplate
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
volumes:
  postgres_data:
```

- [ ] **Step 2: Update README.md**

Replace contents with:

```markdown
# dotnet-cqrs-template

A .NET 10 CQRS template with ASP.NET Core minimal APIs, EF Core + Postgres, MediatR, FluentValidation, Mapperly, and Serilog.

## Prerequisites

- .NET 10 SDK
- Docker (for Postgres)

## Setup

**1. Start Postgres:**
```
docker-compose up -d
```

**2. Set the connection string (stored in user-secrets, never committed):**
```
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=backendtemplate;Username=postgres;Password=postgres" --project backend/src/BackendTemplate.Api
```

**3. Apply migrations:**
```
dotnet ef database update --project backend/src/BackendTemplate.Infrastructure --startup-project backend/src/BackendTemplate.Api
```

**4. Run the API:**
```
dotnet watch --project backend/src/BackendTemplate.Api
```

OpenAPI spec: `http://localhost:5000/openapi/v1.json`  
Scalar UI (dev only): `http://localhost:5000/scalar/v1`  
Health check: `http://localhost:5000/health`

## Development

**Build:**
```
dotnet build backend/BackendTemplate.slnx
```

**Test:**
```
dotnet test backend/BackendTemplate.slnx
```

**Add migration:**
```
dotnet ef migrations add <Name> --project backend/src/BackendTemplate.Infrastructure --startup-project backend/src/BackendTemplate.Api
```
```

- [ ] **Step 3: Commit**

```bash
git add docker-compose.yml README.md
git commit -m "feat: add docker-compose.yml and setup README"
```

---

## Task 9: Verify Build and Tests

- [ ] **Step 1: Full build**

Run: `dotnet build backend/BackendTemplate.slnx`
Expected: 0 errors.

- [ ] **Step 2: Run all tests**

Run: `dotnet test backend/BackendTemplate.slnx`
Expected: All tests pass (44+ tests — no regressions from Plans 1–3).

If Infrastructure.Tests fail with "Docker is either not running or misconfigured", start Docker Desktop first.

- [ ] **Step 3: Confirm no sensitive data committed**

Run: `git log --oneline -10`
Verify `appsettings.Development.json` was not committed with real connection strings.

- [ ] **Step 4: Final commit if any fixes needed**

```bash
git add -p  # stage only what changed
git commit -m "fix: <describe fix>"
```
