# Backend Scaffold — Plan 2: Application Layer

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the Application layer — CQRS commands/queries/handlers, validators, pipeline behaviors, and DI registration — so that `dotnet test backend/BackendTemplate.slnx` passes 39 tests green.

**Architecture:** One folder per feature slice in `BackendTemplate.Application/TodoItems/`. Query handlers return read-model DTOs directly; command handlers return `Result<TodoItem>` or `Result`. An MediatR pipeline runs `LoggingBehavior` then `ValidationBehavior` before every handler. All tests use NSubstitute strict mocks — repositories and `IUnitOfWork` are never wired to a real database.

**Tech Stack:** .NET 10, C# 13, MediatR 12.x, FluentValidation 11.x, NSubstitute 5.x, xUnit 2.x

**Prerequisite:** Plan 1 complete — Domain layer builds and 22 Domain.Tests pass.

---

## File Map

```
backend/
  src/
    BackendTemplate.Application/
      BackendTemplate.Application.csproj           ← MediatR + FluentValidation deps
      Common/
        IRepository.cs                             ← generic base interface
        IUnitOfWork.cs                             ← SaveChangesAsync
        Page.cs                                    ← record Page<T>(Items, Total, Page, PageSize)
      TodoItems/
        ITodoItemRepository.cs                     ← extends IRepository, adds GetByStatusAsync
        CreateTodoItem/
          CreateTodoItemCommand.cs                 ← IRequest<Result<TodoItem>>
          CreateTodoItemCommandHandler.cs
          CreateTodoItemCommandValidator.cs
        GetTodoItem/
          TodoItemDto.cs                           ← read model record
          GetTodoItemQuery.cs                      ← IRequest<Result<TodoItemDto>>
          GetTodoItemQueryHandler.cs
        GetTodoItems/
          GetTodoItemsQuery.cs                     ← IRequest<Page<TodoItemDto>>
          GetTodoItemsQueryHandler.cs
        UpdateTodoItem/
          UpdateTodoItemCommand.cs                 ← nullable fields, null=skip, ""=clear
          UpdateTodoItemCommandHandler.cs
          UpdateTodoItemCommandValidator.cs
        DeleteTodoItem/
          DeleteTodoItemCommand.cs                 ← IRequest<Result>
          DeleteTodoItemCommandHandler.cs
      Behaviors/
        LoggingBehavior.cs                         ← logs request name + elapsed ms
        ValidationBehavior.cs                      ← throws ValidationException on failure
      DependencyInjection.cs                       ← AddApplication() extension method
  tests/
    BackendTemplate.Application.Tests/
      BackendTemplate.Application.Tests.csproj    ← xUnit + NSubstitute + FluentValidation.TestHelper
      TodoItems/
        GetTodoItemQueryHandlerTests.cs            ← 2 tests
        GetTodoItemsQueryHandlerTests.cs           ← 2 tests
        CreateTodoItemCommandHandlerTests.cs       ← 2 tests
        UpdateTodoItemCommandHandlerTests.cs       ← 5 tests
        DeleteTodoItemCommandHandlerTests.cs       ← 2 tests
      Behaviors/
        ValidationBehaviorTests.cs                ← 3 tests
        LoggingBehaviorTests.cs                   ← 1 test
```

---

## Task 1: Application and Application.Tests project setup

**Files:**
- Create: `backend/src/BackendTemplate.Application/BackendTemplate.Application.csproj`
- Create: `backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj`

- [ ] **Step 1: Create project directories**

```powershell
New-Item -ItemType Directory -Force -Path "backend/src/BackendTemplate.Application"
New-Item -ItemType Directory -Force -Path "backend/tests/BackendTemplate.Application.Tests"
```

- [ ] **Step 2: Write BackendTemplate.Application.csproj**

Create `backend/src/BackendTemplate.Application/BackendTemplate.Application.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.4.1" />
    <PackageReference Include="FluentValidation" Version="11.10.0" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.10.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/BackendTemplate.Domain/BackendTemplate.Domain.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Write BackendTemplate.Application.Tests.csproj**

Create `backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <Using Include="Xunit" />
    <Using Include="NSubstitute" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="NSubstitute" Version="5.1.0" />
    <PackageReference Include="FluentValidation.TestHelper" Version="11.10.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/BackendTemplate.Application/BackendTemplate.Application.csproj" />
    <ProjectReference Include="../../src/BackendTemplate.Domain/BackendTemplate.Domain.csproj" />
    <ProjectReference Include="../BackendTemplate.Testing.Common/BackendTemplate.Testing.Common.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Register both projects in the solution**

```powershell
dotnet sln backend/BackendTemplate.slnx add backend/src/BackendTemplate.Application/BackendTemplate.Application.csproj
dotnet sln backend/BackendTemplate.slnx add backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

- [ ] **Step 5: Verify build**

```powershell
dotnet build backend/BackendTemplate.slnx
```

Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```powershell
git add backend/src/BackendTemplate.Application/ backend/tests/BackendTemplate.Application.Tests/
git commit -m "chore: add Application and Application.Tests projects"
```

---

## Task 2: Common infrastructure — IRepository, IUnitOfWork, Page\<T\>, ITodoItemRepository

**Files:**
- Create: `backend/src/BackendTemplate.Application/Common/IRepository.cs`
- Create: `backend/src/BackendTemplate.Application/Common/IUnitOfWork.cs`
- Create: `backend/src/BackendTemplate.Application/Common/Page.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/ITodoItemRepository.cs`

These are interfaces — no logic to TDD. They define the contract that Infrastructure will implement.

- [ ] **Step 1: Create Common and TodoItems directories**

```powershell
New-Item -ItemType Directory -Force -Path "backend/src/BackendTemplate.Application/Common"
New-Item -ItemType Directory -Force -Path "backend/src/BackendTemplate.Application/TodoItems"
```

- [ ] **Step 2: Write IRepository.cs**

Create `backend/src/BackendTemplate.Application/Common/IRepository.cs`:

```csharp
namespace BackendTemplate.Application.Common;

public interface IRepository<T, TId> where T : class
{
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<Page<T>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}
```

- [ ] **Step 3: Write IUnitOfWork.cs**

Create `backend/src/BackendTemplate.Application/Common/IUnitOfWork.cs`:

```csharp
namespace BackendTemplate.Application.Common;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

- [ ] **Step 4: Write Page.cs**

Create `backend/src/BackendTemplate.Application/Common/Page.cs`:

```csharp
namespace BackendTemplate.Application.Common;

public record Page<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
```

- [ ] **Step 5: Write ITodoItemRepository.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/ITodoItemRepository.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;

public interface ITodoItemRepository : IRepository<TodoItem, TodoItemId>
{
    Task<Page<TodoItem>> GetByStatusAsync(
        TodoStatus status,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
```

- [ ] **Step 6: Build to verify**

```powershell
dotnet build backend/BackendTemplate.slnx
```

Expected: `Build succeeded.`

- [ ] **Step 7: Commit**

```powershell
git add backend/src/BackendTemplate.Application/Common/ backend/src/BackendTemplate.Application/TodoItems/ITodoItemRepository.cs
git commit -m "feat: add IRepository, IUnitOfWork, Page<T>, and ITodoItemRepository"
```

---

## Task 3: GetTodoItem slice (TDD)

**Files:**
- Create: `backend/tests/BackendTemplate.Application.Tests/TodoItems/GetTodoItemQueryHandlerTests.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/GetTodoItem/TodoItemDto.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/GetTodoItem/GetTodoItemQuery.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/GetTodoItem/GetTodoItemQueryHandler.cs`

- [ ] **Step 1: Create directories**

```powershell
New-Item -ItemType Directory -Force -Path "backend/src/BackendTemplate.Application/TodoItems/GetTodoItem"
New-Item -ItemType Directory -Force -Path "backend/tests/BackendTemplate.Application.Tests/TodoItems"
```

- [ ] **Step 2: Write failing tests**

Create `backend/tests/BackendTemplate.Application.Tests/TodoItems/GetTodoItemQueryHandlerTests.cs`:

```csharp
namespace BackendTemplate.Application.Tests.TodoItems;

using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.GetTodoItem;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Testing.Common.Builders;

public sealed class GetTodoItemQueryHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();

    [Fact]
    public async Task Handle_GivenExistingId_ThenReturnsTodoItemDto()
    {
        var item = new TodoItemBuilder().WithTitle("Buy milk").Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new GetTodoItemQueryHandler(_repository);

        var result = await handler.Handle(new GetTodoItemQuery(item.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(item.Id.Value, result.Value.Id);
        Assert.Equal("Buy milk", result.Value.Title);
    }

    [Fact]
    public async Task Handle_GivenNonExistentId_ThenReturnsNotFoundResult()
    {
        var id = new TodoItemId(Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TodoItem?)null);
        var handler = new GetTodoItemQueryHandler(_repository);

        var result = await handler.Handle(new GetTodoItemQuery(id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }
}
```

- [ ] **Step 3: Run tests — verify they fail to compile**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: build error — `GetTodoItemQueryHandler`, `GetTodoItemQuery`, `TodoItemDto` not found.

- [ ] **Step 4: Write TodoItemDto.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/GetTodoItem/TodoItemDto.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.GetTodoItem;

using BackendTemplate.Domain.TodoItems;

public record TodoItemDto(
    Guid Id,
    string Title,
    string? Description,
    TodoStatus Status,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
```

- [ ] **Step 5: Write GetTodoItemQuery.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/GetTodoItem/GetTodoItemQuery.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.GetTodoItem;

using BackendTemplate.Domain.Common;
using MediatR;

public record GetTodoItemQuery(TodoItemId Id) : IRequest<Result<TodoItemDto>>;
```

- [ ] **Step 6: Write GetTodoItemQueryHandler.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/GetTodoItem/GetTodoItemQueryHandler.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.GetTodoItem;

using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public sealed class GetTodoItemQueryHandler : IRequestHandler<GetTodoItemQuery, Result<TodoItemDto>>
{
    private readonly ITodoItemRepository _repository;

    public GetTodoItemQueryHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TodoItemDto>> Handle(GetTodoItemQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            return Result<TodoItemDto>.Failure($"Todo item '{request.Id.Value}' not found.", ErrorKind.NotFound);

        return Result<TodoItemDto>.Success(MapToDto(item));
    }

    private static TodoItemDto MapToDto(TodoItem item) =>
        new(item.Id.Value, item.Title, item.Description, item.Status, item.CreatedAtUtc, item.CompletedAtUtc);
}
```

- [ ] **Step 7: Run tests — verify 2 pass**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 2, Skipped: 0`

- [ ] **Step 8: Commit**

```powershell
git add backend/src/BackendTemplate.Application/TodoItems/GetTodoItem/
git add backend/tests/BackendTemplate.Application.Tests/TodoItems/GetTodoItemQueryHandlerTests.cs
git commit -m "feat: add GetTodoItem query, handler, and TodoItemDto"
```

---

## Task 4: GetTodoItems slice (TDD)

**Files:**
- Create: `backend/tests/BackendTemplate.Application.Tests/TodoItems/GetTodoItemsQueryHandlerTests.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/GetTodoItems/GetTodoItemsQuery.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/GetTodoItems/GetTodoItemsQueryHandler.cs`

- [ ] **Step 1: Create directory**

```powershell
New-Item -ItemType Directory -Force -Path "backend/src/BackendTemplate.Application/TodoItems/GetTodoItems"
```

- [ ] **Step 2: Write failing tests**

Create `backend/tests/BackendTemplate.Application.Tests/TodoItems/GetTodoItemsQueryHandlerTests.cs`:

```csharp
namespace BackendTemplate.Application.Tests.TodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.GetTodoItems;
using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Testing.Common.Builders;

public sealed class GetTodoItemsQueryHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();

    [Fact]
    public async Task Handle_GivenNoStatusFilter_ThenReturnsAllItemsMapped()
    {
        var item = new TodoItemBuilder().WithTitle("Buy milk").Build();
        var page = new Page<TodoItem>([item], 1, 1, 20);
        _repository.GetAllAsync(1, 20, Arg.Any<CancellationToken>()).Returns(page);
        var handler = new GetTodoItemsQueryHandler(_repository);

        var result = await handler.Handle(new GetTodoItemsQuery(1, 20, null), CancellationToken.None);

        Assert.Equal(1, result.Total);
        Assert.Single(result.Items);
        Assert.Equal("Buy milk", result.Items[0].Title);
    }

    [Fact]
    public async Task Handle_GivenStatusFilter_ThenCallsGetByStatus()
    {
        var item = TodoItemBuilder.InProgress().Build();
        var page = new Page<TodoItem>([item], 1, 1, 20);
        _repository.GetByStatusAsync(TodoStatus.InProgress, 1, 20, Arg.Any<CancellationToken>()).Returns(page);
        var handler = new GetTodoItemsQueryHandler(_repository);

        var result = await handler.Handle(new GetTodoItemsQuery(1, 20, TodoStatus.InProgress), CancellationToken.None);

        Assert.Equal(1, result.Total);
        await _repository.Received(1).GetByStatusAsync(TodoStatus.InProgress, 1, 20, Arg.Any<CancellationToken>());
    }
}
```

- [ ] **Step 3: Run tests — verify they fail to compile**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: build error — `GetTodoItemsQueryHandler`, `GetTodoItemsQuery` not found.

- [ ] **Step 4: Write GetTodoItemsQuery.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/GetTodoItems/GetTodoItemsQuery.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.GetTodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems.GetTodoItem;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public record GetTodoItemsQuery(int Page, int PageSize, TodoStatus? Status)
    : IRequest<Page<TodoItemDto>>;
```

- [ ] **Step 5: Write GetTodoItemsQueryHandler.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/GetTodoItems/GetTodoItemsQueryHandler.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.GetTodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems.GetTodoItem;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public sealed class GetTodoItemsQueryHandler : IRequestHandler<GetTodoItemsQuery, Page<TodoItemDto>>
{
    private readonly ITodoItemRepository _repository;

    public GetTodoItemsQueryHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<Page<TodoItemDto>> Handle(GetTodoItemsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Status is not null
            ? await _repository.GetByStatusAsync(request.Status.Value, request.Page, request.PageSize, cancellationToken)
            : await _repository.GetAllAsync(request.Page, request.PageSize, cancellationToken);

        return new Page<TodoItemDto>(
            page.Items.Select(MapToDto).ToList(),
            page.Total,
            page.Page,
            page.PageSize);
    }

    private static TodoItemDto MapToDto(TodoItem item) =>
        new(item.Id.Value, item.Title, item.Description, item.Status, item.CreatedAtUtc, item.CompletedAtUtc);
}
```

- [ ] **Step 6: Run tests — verify 4 pass**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 4, Skipped: 0`

- [ ] **Step 7: Commit**

```powershell
git add backend/src/BackendTemplate.Application/TodoItems/GetTodoItems/
git add backend/tests/BackendTemplate.Application.Tests/TodoItems/GetTodoItemsQueryHandlerTests.cs
git commit -m "feat: add GetTodoItems query and handler"
```

---

## Task 5: CreateTodoItem slice (TDD)

**Files:**
- Create: `backend/tests/BackendTemplate.Application.Tests/TodoItems/CreateTodoItemCommandHandlerTests.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/CreateTodoItem/CreateTodoItemCommand.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/CreateTodoItem/CreateTodoItemCommandHandler.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/CreateTodoItem/CreateTodoItemCommandValidator.cs`

- [ ] **Step 1: Create directory**

```powershell
New-Item -ItemType Directory -Force -Path "backend/src/BackendTemplate.Application/TodoItems/CreateTodoItem"
```

- [ ] **Step 2: Write failing tests**

Create `backend/tests/BackendTemplate.Application.Tests/TodoItems/CreateTodoItemCommandHandlerTests.cs`:

```csharp
namespace BackendTemplate.Application.Tests.TodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.CreateTodoItem;

public sealed class CreateTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_GivenValidCommand_ThenCreatesAndReturnsTodoItem()
    {
        var handler = new CreateTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(
            new CreateTodoItemCommand("Buy milk", "From the store"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Buy milk", result.Value.Title);
        Assert.Equal("From the store", result.Value.Description);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ThenCallsSaveChanges()
    {
        var handler = new CreateTodoItemCommandHandler(_repository, _unitOfWork);

        await handler.Handle(new CreateTodoItemCommand("Buy milk", null), CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
```

- [ ] **Step 3: Run tests — verify they fail to compile**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: build error — `CreateTodoItemCommandHandler`, `CreateTodoItemCommand` not found.

- [ ] **Step 4: Write CreateTodoItemCommand.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/CreateTodoItem/CreateTodoItemCommand.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.CreateTodoItem;

using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public record CreateTodoItemCommand(string Title, string? Description)
    : IRequest<Result<TodoItem>>;
```

- [ ] **Step 5: Write CreateTodoItemCommandHandler.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/CreateTodoItem/CreateTodoItemCommandHandler.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.CreateTodoItem;

using BackendTemplate.Application.Common;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public sealed class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, Result<TodoItem>>
{
    private readonly ITodoItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTodoItemCommandHandler(ITodoItemRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TodoItem>> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = TodoItem.Create(request.Title, request.Description);
        await _repository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TodoItem>.Success(item);
    }
}
```

- [ ] **Step 6: Write CreateTodoItemCommandValidator.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/CreateTodoItem/CreateTodoItemCommandValidator.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.CreateTodoItem;

using BackendTemplate.Domain.TodoItems;
using FluentValidation;

public sealed class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(TodoItem.TitleMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(TodoItem.DescriptionMaxLength)
            .When(x => x.Description is not null);
    }
}
```

- [ ] **Step 7: Run tests — verify 6 pass**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 6, Skipped: 0`

- [ ] **Step 8: Commit**

```powershell
git add backend/src/BackendTemplate.Application/TodoItems/CreateTodoItem/
git add backend/tests/BackendTemplate.Application.Tests/TodoItems/CreateTodoItemCommandHandlerTests.cs
git commit -m "feat: add CreateTodoItem command, handler, and validator"
```

---

## Task 6: UpdateTodoItem slice (TDD)

**Files:**
- Create: `backend/tests/BackendTemplate.Application.Tests/TodoItems/UpdateTodoItemCommandHandlerTests.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/UpdateTodoItem/UpdateTodoItemCommand.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/UpdateTodoItem/UpdateTodoItemCommandHandler.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/UpdateTodoItem/UpdateTodoItemCommandValidator.cs`

- [ ] **Step 1: Create directory**

```powershell
New-Item -ItemType Directory -Force -Path "backend/src/BackendTemplate.Application/TodoItems/UpdateTodoItem"
```

- [ ] **Step 2: Write failing tests**

Create `backend/tests/BackendTemplate.Application.Tests/TodoItems/UpdateTodoItemCommandHandlerTests.cs`:

```csharp
namespace BackendTemplate.Application.Tests.TodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.UpdateTodoItem;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Testing.Common.Builders;

public sealed class UpdateTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_GivenNonExistentId_ThenReturnsNotFoundResult()
    {
        var id = new TodoItemId(Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TodoItem?)null);
        var handler = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(
            new UpdateTodoItemCommand(id, "New title", null, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Handle_GivenNullTitle_ThenTitleUnchanged()
    {
        var item = new TodoItemBuilder().WithTitle("Original").Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(
            new UpdateTodoItemCommand(item.Id, null, null, null),
            CancellationToken.None);

        Assert.Equal("Original", result.Value.Title);
    }

    [Fact]
    public async Task Handle_GivenNewTitle_ThenTitleUpdated()
    {
        var item = new TodoItemBuilder().WithTitle("Original").Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(
            new UpdateTodoItemCommand(item.Id, "Updated", null, null),
            CancellationToken.None);

        Assert.Equal("Updated", result.Value.Title);
    }

    [Fact]
    public async Task Handle_GivenEmptyDescription_ThenDescriptionCleared()
    {
        var item = new TodoItemBuilder().WithDescription("Some description").Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(
            new UpdateTodoItemCommand(item.Id, null, "", null),
            CancellationToken.None);

        Assert.Null(result.Value.Description);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ThenCallsSaveChanges()
    {
        var item = new TodoItemBuilder().Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);

        await handler.Handle(
            new UpdateTodoItemCommand(item.Id, "New title", null, null),
            CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
```

- [ ] **Step 3: Run tests — verify they fail to compile**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: build error — `UpdateTodoItemCommandHandler`, `UpdateTodoItemCommand` not found.

- [ ] **Step 4: Write UpdateTodoItemCommand.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/UpdateTodoItem/UpdateTodoItemCommand.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.UpdateTodoItem;

using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public record UpdateTodoItemCommand(
    TodoItemId Id,
    string? Title,
    string? Description,
    TodoStatus? Status) : IRequest<Result<TodoItem>>;
```

- [ ] **Step 5: Write UpdateTodoItemCommandHandler.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/UpdateTodoItem/UpdateTodoItemCommandHandler.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.UpdateTodoItem;

using BackendTemplate.Application.Common;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public sealed class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand, Result<TodoItem>>
{
    private readonly ITodoItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTodoItemCommandHandler(ITodoItemRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TodoItem>> Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            return Result<TodoItem>.Failure($"Todo item '{request.Id.Value}' not found.", ErrorKind.NotFound);

        if (request.Title is not null)
            item.UpdateTitle(request.Title);

        if (request.Description is not null)
            item.UpdateDescription(request.Description.Length == 0 ? null : request.Description);

        if (request.Status is not null)
            item.UpdateStatus(request.Status.Value);

        _repository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TodoItem>.Success(item);
    }
}
```

- [ ] **Step 6: Write UpdateTodoItemCommandValidator.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/UpdateTodoItem/UpdateTodoItemCommandValidator.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.UpdateTodoItem;

using BackendTemplate.Domain.TodoItems;
using FluentValidation;

public sealed class UpdateTodoItemCommandValidator : AbstractValidator<UpdateTodoItemCommand>
{
    public UpdateTodoItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(TodoItem.TitleMaxLength)
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(TodoItem.DescriptionMaxLength)
            .When(x => x.Description is not null && x.Description.Length > 0);
    }
}
```

- [ ] **Step 7: Run tests — verify 11 pass**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 11, Skipped: 0`

- [ ] **Step 8: Commit**

```powershell
git add backend/src/BackendTemplate.Application/TodoItems/UpdateTodoItem/
git add backend/tests/BackendTemplate.Application.Tests/TodoItems/UpdateTodoItemCommandHandlerTests.cs
git commit -m "feat: add UpdateTodoItem command, handler, and validator"
```

---

## Task 7: DeleteTodoItem slice (TDD)

**Files:**
- Create: `backend/tests/BackendTemplate.Application.Tests/TodoItems/DeleteTodoItemCommandHandlerTests.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/DeleteTodoItem/DeleteTodoItemCommand.cs`
- Create: `backend/src/BackendTemplate.Application/TodoItems/DeleteTodoItem/DeleteTodoItemCommandHandler.cs`

- [ ] **Step 1: Create directory**

```powershell
New-Item -ItemType Directory -Force -Path "backend/src/BackendTemplate.Application/TodoItems/DeleteTodoItem"
```

- [ ] **Step 2: Write failing tests**

Create `backend/tests/BackendTemplate.Application.Tests/TodoItems/DeleteTodoItemCommandHandlerTests.cs`:

```csharp
namespace BackendTemplate.Application.Tests.TodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.DeleteTodoItem;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Testing.Common.Builders;

public sealed class DeleteTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_GivenExistingId_ThenReturnsSuccess()
    {
        var item = new TodoItemBuilder().Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new DeleteTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(new DeleteTodoItemCommand(item.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_GivenNonExistentId_ThenReturnsNotFoundResult()
    {
        var id = new TodoItemId(Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TodoItem?)null);
        var handler = new DeleteTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(new DeleteTodoItemCommand(id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }
}
```

- [ ] **Step 3: Run tests — verify they fail to compile**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: build error — `DeleteTodoItemCommandHandler`, `DeleteTodoItemCommand` not found.

- [ ] **Step 4: Write DeleteTodoItemCommand.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/DeleteTodoItem/DeleteTodoItemCommand.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.DeleteTodoItem;

using BackendTemplate.Domain.Common;
using MediatR;

public record DeleteTodoItemCommand(TodoItemId Id) : IRequest<Result>;
```

- [ ] **Step 5: Write DeleteTodoItemCommandHandler.cs**

Create `backend/src/BackendTemplate.Application/TodoItems/DeleteTodoItem/DeleteTodoItemCommandHandler.cs`:

```csharp
namespace BackendTemplate.Application.TodoItems.DeleteTodoItem;

using BackendTemplate.Application.Common;
using BackendTemplate.Domain.Common;
using MediatR;

public sealed class DeleteTodoItemCommandHandler : IRequestHandler<DeleteTodoItemCommand, Result>
{
    private readonly ITodoItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTodoItemCommandHandler(ITodoItemRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            return Result.Failure($"Todo item '{request.Id.Value}' not found.", ErrorKind.NotFound);

        _repository.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
```

- [ ] **Step 6: Run tests — verify 13 pass**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 13, Skipped: 0`

- [ ] **Step 7: Commit**

```powershell
git add backend/src/BackendTemplate.Application/TodoItems/DeleteTodoItem/
git add backend/tests/BackendTemplate.Application.Tests/TodoItems/DeleteTodoItemCommandHandlerTests.cs
git commit -m "feat: add DeleteTodoItem command and handler"
```

---

## Task 8: Pipeline behaviors (TDD)

**Files:**
- Create: `backend/tests/BackendTemplate.Application.Tests/Behaviors/ValidationBehaviorTests.cs`
- Create: `backend/tests/BackendTemplate.Application.Tests/Behaviors/LoggingBehaviorTests.cs`
- Create: `backend/src/BackendTemplate.Application/Behaviors/LoggingBehavior.cs`
- Create: `backend/src/BackendTemplate.Application/Behaviors/ValidationBehavior.cs`

- [ ] **Step 1: Create directories**

```powershell
New-Item -ItemType Directory -Force -Path "backend/src/BackendTemplate.Application/Behaviors"
New-Item -ItemType Directory -Force -Path "backend/tests/BackendTemplate.Application.Tests/Behaviors"
```

- [ ] **Step 2: Write failing ValidationBehavior tests**

Create `backend/tests/BackendTemplate.Application.Tests/Behaviors/ValidationBehaviorTests.cs`:

```csharp
namespace BackendTemplate.Application.Tests.Behaviors;

using BackendTemplate.Application.Behaviors;
using FluentValidation;
using MediatR;

public sealed class ValidationBehaviorTests
{
    private sealed record TestRequest(string Value) : IRequest<string>;

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    [Fact]
    public async Task Handle_GivenRequestWithNoValidators_ThenCallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var nextCalled = false;
        Task<string> Next() { nextCalled = true; return Task.FromResult("ok"); }

        await behavior.Handle(new TestRequest("value"), Next, CancellationToken.None);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_ThenCallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new TestRequestValidator()]);
        var nextCalled = false;
        Task<string> Next() { nextCalled = true; return Task.FromResult("ok"); }

        await behavior.Handle(new TestRequest("hello"), Next, CancellationToken.None);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_GivenInvalidRequest_ThenThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new TestRequestValidator()]);
        Task<string> Next() => Task.FromResult("ok");

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new TestRequest(""), Next, CancellationToken.None));
    }
}
```

- [ ] **Step 3: Write failing LoggingBehavior test**

Create `backend/tests/BackendTemplate.Application.Tests/Behaviors/LoggingBehaviorTests.cs`:

```csharp
namespace BackendTemplate.Application.Tests.Behaviors;

using BackendTemplate.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed class LoggingBehaviorTests
{
    private sealed record TestRequest(string Value) : IRequest<string>;

    [Fact]
    public async Task Handle_Always_ThenCallsNext()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);
        var nextCalled = false;
        Task<string> Next() { nextCalled = true; return Task.FromResult("ok"); }

        await behavior.Handle(new TestRequest("value"), Next, CancellationToken.None);

        Assert.True(nextCalled);
    }
}
```

- [ ] **Step 4: Run tests — verify they fail to compile**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: build error — `ValidationBehavior`, `LoggingBehavior` not found.

- [ ] **Step 5: Write LoggingBehavior.cs**

Create `backend/src/BackendTemplate.Application/Behaviors/LoggingBehavior.cs`:

```csharp
namespace BackendTemplate.Application.Behaviors;

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();
        _logger.LogInformation("{RequestName} handled in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
```

- [ ] **Step 6: Write ValidationBehavior.cs**

Create `backend/src/BackendTemplate.Application/Behaviors/ValidationBehavior.cs`:

```csharp
namespace BackendTemplate.Application.Behaviors;

using FluentValidation;
using MediatR;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

- [ ] **Step 7: Run tests — verify 17 pass**

```powershell
dotnet test backend/tests/BackendTemplate.Application.Tests/BackendTemplate.Application.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 17, Skipped: 0`

- [ ] **Step 8: Commit**

```powershell
git add backend/src/BackendTemplate.Application/Behaviors/
git add backend/tests/BackendTemplate.Application.Tests/Behaviors/
git commit -m "feat: add LoggingBehavior and ValidationBehavior pipeline behaviors"
```

---

## Task 9: DependencyInjection + final verification

**Files:**
- Create: `backend/src/BackendTemplate.Application/DependencyInjection.cs`

- [ ] **Step 1: Write DependencyInjection.cs**

Create `backend/src/BackendTemplate.Application/DependencyInjection.cs`:

```csharp
namespace BackendTemplate.Application;

using BackendTemplate.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
```

- [ ] **Step 2: Build entire solution**

```powershell
dotnet build backend/BackendTemplate.slnx
```

Expected: `Build succeeded.`

- [ ] **Step 3: Run all tests**

```powershell
dotnet test backend/BackendTemplate.slnx
```

Expected: `Passed! - Failed: 0, Passed: 39, Skipped: 0, Total: 39`

- [ ] **Step 4: Commit**

```powershell
git add backend/src/BackendTemplate.Application/DependencyInjection.cs
git commit -m "feat: add AddApplication DI registration"
```

---

## Exit condition

```powershell
dotnet test backend/BackendTemplate.slnx
```

Expected:
```
Passed! - Failed: 0, Passed: 39, Skipped: 0, Total: 39
```

Application layer has zero infrastructure dependencies. All handlers tested with NSubstitute mocks. Plan 3 (Infrastructure layer) can now begin.
