# Backend Scaffold — Plan 1: Domain Layer

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Scaffold the solution structure and implement the Domain layer with full test coverage, so that `dotnet test backend/tests/BackendTemplate.Domain.Tests` passes green.

**Architecture:** Clean solution with `Directory.Build.props` sharing common MSBuild properties. Domain layer has zero framework dependencies — plain C# classes, enums, and record structs. `Result<T>` and `Result` provide explicit error handling without exceptions. `TodoItem` entity enforces invariants via a static factory and domain methods.

**Tech Stack:** .NET 10, C# 13, xUnit 2.x

---

## File Map

```
backend/
  Directory.Build.props                                          ← shared MSBuild props (TFM, nullable, lang)
  BackendTemplate.sln
  src/
    BackendTemplate.Domain/
      BackendTemplate.Domain.csproj                             ← no dependencies, no NuGet packages
      Common/
        ErrorKind.cs                                            ← enum: Validation | NotFound | Conflict
        Result.cs                                               ← Result and Result<T> (both in one file)
        TodoItemId.cs                                           ← record struct TodoItemId(Guid Value)
      TodoItems/
        TodoStatus.cs                                           ← enum: Pending | InProgress | Done
        TodoItem.cs                                             ← entity with Create factory + domain methods
  tests/
    BackendTemplate.Testing.Common/
      BackendTemplate.Testing.Common.csproj                     ← references Domain only
      Builders/
        TodoItemBuilder.cs                                      ← fluent builder + InProgress/Done recipes
    BackendTemplate.Domain.Tests/
      BackendTemplate.Domain.Tests.csproj                       ← references Domain + Testing.Common
      Common/
        ResultTests.cs                                          ← tests for Result<T> and Result
      TodoItems/
        TodoItemTests.cs                                        ← tests for TodoItem entity behaviour
```

---

## Task 1: Solution and project scaffold

**Files:**
- Create: `backend/Directory.Build.props`
- Create: `backend/BackendTemplate.sln`
- Create: `backend/src/BackendTemplate.Domain/BackendTemplate.Domain.csproj`
- Create: `backend/tests/BackendTemplate.Testing.Common/BackendTemplate.Testing.Common.csproj`
- Create: `backend/tests/BackendTemplate.Domain.Tests/BackendTemplate.Domain.Tests.csproj`

- [ ] **Step 1: Create the solution file**

```powershell
dotnet new sln -n BackendTemplate -o backend
```

Expected: `backend/BackendTemplate.sln` created.

- [ ] **Step 2: Create the shared MSBuild properties file**

Create `backend/Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>13</LangVersion>
  </PropertyGroup>
</Project>
```

- [ ] **Step 3: Create project directories**

```powershell
New-Item -ItemType Directory -Force -Path backend/src/BackendTemplate.Domain
New-Item -ItemType Directory -Force -Path backend/tests/BackendTemplate.Testing.Common
New-Item -ItemType Directory -Force -Path backend/tests/BackendTemplate.Domain.Tests
```

- [ ] **Step 4: Write BackendTemplate.Domain.csproj**

Create `backend/src/BackendTemplate.Domain/BackendTemplate.Domain.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
</Project>
```

All properties (TFM, nullable, lang) come from `Directory.Build.props`.

- [ ] **Step 5: Write BackendTemplate.Testing.Common.csproj**

Create `backend/tests/BackendTemplate.Testing.Common/BackendTemplate.Testing.Common.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="../../src/BackendTemplate.Domain/BackendTemplate.Domain.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 6: Write BackendTemplate.Domain.Tests.csproj**

Create `backend/tests/BackendTemplate.Domain.Tests/BackendTemplate.Domain.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/BackendTemplate.Domain/BackendTemplate.Domain.csproj" />
    <ProjectReference Include="../BackendTemplate.Testing.Common/BackendTemplate.Testing.Common.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 7: Register all three projects in the solution**

```powershell
dotnet sln backend/BackendTemplate.sln add backend/src/BackendTemplate.Domain/BackendTemplate.Domain.csproj
dotnet sln backend/BackendTemplate.sln add backend/tests/BackendTemplate.Testing.Common/BackendTemplate.Testing.Common.csproj
dotnet sln backend/BackendTemplate.sln add backend/tests/BackendTemplate.Domain.Tests/BackendTemplate.Domain.Tests.csproj
```

- [ ] **Step 8: Verify the solution builds**

```powershell
dotnet build backend/BackendTemplate.sln
```

Expected: `Build succeeded.` (zero errors, zero warnings).

- [ ] **Step 9: Commit**

```powershell
git add backend/
git commit -m "chore: scaffold solution with Domain, Testing.Common, and Domain.Tests projects"
```

---

## Task 2: Result types (TDD)

**Files:**
- Create: `backend/tests/BackendTemplate.Domain.Tests/Common/ResultTests.cs`
- Create: `backend/src/BackendTemplate.Domain/Common/ErrorKind.cs`
- Create: `backend/src/BackendTemplate.Domain/Common/Result.cs`

- [ ] **Step 1: Create the test directory and write the failing tests**

Create `backend/tests/BackendTemplate.Domain.Tests/Common/ResultTests.cs`:

```csharp
namespace BackendTemplate.Domain.Tests.Common;

using BackendTemplate.Domain.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_GivenValue_ThenIsSuccessTrue()
    {
        var result = Result<string>.Success("hello");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Success_GivenValue_ThenAccessingErrorThrows()
    {
        var result = Result<string>.Success("hello");

        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void Success_GivenValue_ThenAccessingKindThrows()
    {
        var result = Result<string>.Success("hello");

        Assert.Throws<InvalidOperationException>(() => _ = result.Kind);
    }

    [Fact]
    public void Failure_GivenErrorAndKind_ThenIsFailureTrue()
    {
        var result = Result<string>.Failure("not found", ErrorKind.NotFound);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("not found", result.Error);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public void Failure_GivenError_ThenAccessingValueThrows()
    {
        var result = Result<string>.Failure("error");

        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Failure_WhenKindNotSpecified_ThenDefaultsToValidation()
    {
        var result = Result<string>.Failure("invalid");

        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public void NonGenericSuccess_ThenIsSuccessTrue()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void NonGenericSuccess_ThenAccessingErrorThrows()
    {
        var result = Result.Success();

        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void NonGenericFailure_GivenErrorAndKind_ThenIsFailureTrue()
    {
        var result = Result.Failure("conflict", ErrorKind.Conflict);

        Assert.True(result.IsFailure);
        Assert.Equal("conflict", result.Error);
        Assert.Equal(ErrorKind.Conflict, result.Kind);
    }

    [Fact]
    public void NonGenericFailure_WhenKindNotSpecified_ThenDefaultsToValidation()
    {
        var result = Result.Failure("invalid");

        Assert.Equal(ErrorKind.Validation, result.Kind);
    }
}
```

- [ ] **Step 2: Run tests — verify they fail to compile**

```powershell
dotnet test backend/tests/BackendTemplate.Domain.Tests/BackendTemplate.Domain.Tests.csproj
```

Expected: build error — `Result<T>`, `Result`, and `ErrorKind` not found.

- [ ] **Step 3: Write ErrorKind.cs**

Create `backend/src/BackendTemplate.Domain/Common/ErrorKind.cs`:

```csharp
namespace BackendTemplate.Domain.Common;

public enum ErrorKind
{
    Validation,
    NotFound,
    Conflict
}
```

- [ ] **Step 4: Write Result.cs (contains both Result and Result<T>)**

Create `backend/src/BackendTemplate.Domain/Common/Result.cs`:

```csharp
namespace BackendTemplate.Domain.Common;

public sealed class Result
{
    private readonly string? _error;
    private readonly ErrorKind? _kind;

    private Result(bool isSuccess, string? error, ErrorKind? kind)
    {
        IsSuccess = isSuccess;
        _error = error;
        _kind = kind;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public string Error => IsSuccess
        ? throw new InvalidOperationException("Cannot access Error on a successful result.")
        : _error!;

    public ErrorKind Kind => IsSuccess
        ? throw new InvalidOperationException("Cannot access Kind on a successful result.")
        : _kind!.Value;

    public static Result Success() => new(true, null, null);

    public static Result Failure(string error, ErrorKind kind = ErrorKind.Validation) =>
        new(false, error, kind);
}

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly string? _error;
    private readonly ErrorKind? _kind;

    private Result(bool isSuccess, T? value, string? error, ErrorKind? kind)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
        _kind = kind;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public string Error => IsSuccess
        ? throw new InvalidOperationException("Cannot access Error on a successful result.")
        : _error!;

    public ErrorKind Kind => IsSuccess
        ? throw new InvalidOperationException("Cannot access Kind on a successful result.")
        : _kind!.Value;

    public static Result<T> Success(T value) => new(true, value, null, null);

    public static Result<T> Failure(string error, ErrorKind kind = ErrorKind.Validation) =>
        new(false, default, error, kind);
}
```

- [ ] **Step 5: Run tests — verify they pass**

```powershell
dotnet test backend/tests/BackendTemplate.Domain.Tests/BackendTemplate.Domain.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 10, Skipped: 0`

- [ ] **Step 6: Commit**

```powershell
git add backend/src/BackendTemplate.Domain/Common/
git add backend/tests/BackendTemplate.Domain.Tests/Common/
git commit -m "feat: add Result<T>, Result, and ErrorKind to Domain.Common"
```

---

## Task 3: Value types — TodoItemId and TodoStatus

**Files:**
- Create: `backend/src/BackendTemplate.Domain/Common/TodoItemId.cs`
- Create: `backend/src/BackendTemplate.Domain/TodoItems/TodoStatus.cs`

These types have no logic to TDD — they are structural primitives. They will be exercised indirectly by `TodoItemTests` in Task 4.

- [ ] **Step 1: Create the TodoItems source directory**

```powershell
New-Item -ItemType Directory -Force -Path backend/src/BackendTemplate.Domain/TodoItems
```

- [ ] **Step 2: Write TodoItemId.cs**

Create `backend/src/BackendTemplate.Domain/Common/TodoItemId.cs`:

```csharp
namespace BackendTemplate.Domain.Common;

public record struct TodoItemId(Guid Value);
```

- [ ] **Step 3: Write TodoStatus.cs**

Create `backend/src/BackendTemplate.Domain/TodoItems/TodoStatus.cs`:

```csharp
namespace BackendTemplate.Domain.TodoItems;

public enum TodoStatus
{
    Pending,
    InProgress,
    Done
}
```

- [ ] **Step 4: Build to verify**

```powershell
dotnet build backend/BackendTemplate.sln
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```powershell
git add backend/src/BackendTemplate.Domain/Common/TodoItemId.cs
git add backend/src/BackendTemplate.Domain/TodoItems/TodoStatus.cs
git commit -m "feat: add TodoItemId strongly-typed ID and TodoStatus enum"
```

---

## Task 4: TodoItem entity (TDD)

**Files:**
- Create: `backend/tests/BackendTemplate.Domain.Tests/TodoItems/TodoItemTests.cs`
- Create: `backend/src/BackendTemplate.Domain/TodoItems/TodoItem.cs`

- [ ] **Step 1: Create the test directory and write failing tests**

Create `backend/tests/BackendTemplate.Domain.Tests/TodoItems/TodoItemTests.cs`:

```csharp
namespace BackendTemplate.Domain.Tests.TodoItems;

using BackendTemplate.Domain.TodoItems;

public sealed class TodoItemTests
{
    [Fact]
    public void Create_GivenTitleAndDescription_ThenCreatesWithPendingStatus()
    {
        var item = TodoItem.Create("Buy milk", "From the store");

        Assert.Equal("Buy milk", item.Title);
        Assert.Equal("From the store", item.Description);
        Assert.Equal(TodoStatus.Pending, item.Status);
        Assert.NotEqual(default, item.Id.Value);
        Assert.Null(item.CompletedAtUtc);
    }

    [Fact]
    public void Create_GivenNoDescription_ThenDescriptionIsNull()
    {
        var item = TodoItem.Create("Buy milk");

        Assert.Null(item.Description);
    }

    [Fact]
    public void Create_ThenCreatedAtUtcIsSet()
    {
        var before = DateTime.UtcNow;
        var item = TodoItem.Create("Buy milk");
        var after = DateTime.UtcNow;

        Assert.InRange(item.CreatedAtUtc, before, after);
    }

    [Fact]
    public void Create_ThenEachCallProducesUniqueId()
    {
        var item1 = TodoItem.Create("Item 1");
        var item2 = TodoItem.Create("Item 2");

        Assert.NotEqual(item1.Id, item2.Id);
    }

    [Fact]
    public void UpdateStatus_GivenDone_ThenSetsCompletedAtUtc()
    {
        var item = TodoItem.Create("Buy milk");

        item.UpdateStatus(TodoStatus.Done);

        Assert.Equal(TodoStatus.Done, item.Status);
        Assert.NotNull(item.CompletedAtUtc);
    }

    [Fact]
    public void UpdateStatus_GivenTransitionFromDoneToPending_ThenClearsCompletedAtUtc()
    {
        var item = TodoItem.Create("Buy milk");
        item.UpdateStatus(TodoStatus.Done);

        item.UpdateStatus(TodoStatus.Pending);

        Assert.Equal(TodoStatus.Pending, item.Status);
        Assert.Null(item.CompletedAtUtc);
    }

    [Fact]
    public void UpdateStatus_GivenInProgress_ThenCompletedAtUtcIsNull()
    {
        var item = TodoItem.Create("Buy milk");

        item.UpdateStatus(TodoStatus.InProgress);

        Assert.Null(item.CompletedAtUtc);
    }

    [Fact]
    public void TitleMaxLength_Is200()
    {
        Assert.Equal(200, TodoItem.TitleMaxLength);
    }

    [Fact]
    public void DescriptionMaxLength_Is2000()
    {
        Assert.Equal(2000, TodoItem.DescriptionMaxLength);
    }

    [Fact]
    public void UpdateTitle_GivenNewTitle_ThenTitleUpdated()
    {
        var item = TodoItem.Create("Old title");

        item.UpdateTitle("New title");

        Assert.Equal("New title", item.Title);
    }

    [Fact]
    public void UpdateDescription_GivenNewDescription_ThenDescriptionUpdated()
    {
        var item = TodoItem.Create("Task", "Old description");

        item.UpdateDescription("New description");

        Assert.Equal("New description", item.Description);
    }

    [Fact]
    public void UpdateDescription_GivenNull_ThenDescriptionCleared()
    {
        var item = TodoItem.Create("Task", "Old description");

        item.UpdateDescription(null);

        Assert.Null(item.Description);
    }
}
```

- [ ] **Step 2: Run tests — verify they fail to compile**

```powershell
dotnet test backend/tests/BackendTemplate.Domain.Tests/BackendTemplate.Domain.Tests.csproj
```

Expected: build error — `TodoItem` not found.

- [ ] **Step 3: Write TodoItem.cs**

Create `backend/src/BackendTemplate.Domain/TodoItems/TodoItem.cs`:

```csharp
namespace BackendTemplate.Domain.TodoItems;

using BackendTemplate.Domain.Common;

public sealed class TodoItem
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private TodoItem() { } // EF Core materialisation

    private TodoItem(TodoItemId id, string title, string? description)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = TodoStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public TodoItemId Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TodoStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public static TodoItem Create(string title, string? description = null) =>
        new(new TodoItemId(Guid.NewGuid()), title, description);

    public void UpdateTitle(string title) => Title = title;

    public void UpdateDescription(string? description) => Description = description;

    public void UpdateStatus(TodoStatus newStatus)
    {
        Status = newStatus;
        CompletedAtUtc = newStatus == TodoStatus.Done ? DateTime.UtcNow : null;
    }
}
```

- [ ] **Step 4: Run tests — verify all pass**

```powershell
dotnet test backend/tests/BackendTemplate.Domain.Tests/BackendTemplate.Domain.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 22, Skipped: 0`

- [ ] **Step 5: Commit**

```powershell
git add backend/src/BackendTemplate.Domain/TodoItems/TodoItem.cs
git add backend/tests/BackendTemplate.Domain.Tests/TodoItems/
git commit -m "feat: add TodoItem entity with Create factory and domain methods"
```

---

## Task 5: TodoItemBuilder

**Files:**
- Create: `backend/tests/BackendTemplate.Testing.Common/Builders/TodoItemBuilder.cs`

`TodoItemBuilder` is test infrastructure — not a subject under test itself. It provides default-valid entities and named recipes for use in Application and Infrastructure test projects (Plans 2 and 3).

- [ ] **Step 1: Create the Builders directory**

```powershell
New-Item -ItemType Directory -Force -Path backend/tests/BackendTemplate.Testing.Common/Builders
```

- [ ] **Step 2: Write TodoItemBuilder.cs**

Create `backend/tests/BackendTemplate.Testing.Common/Builders/TodoItemBuilder.cs`:

```csharp
namespace BackendTemplate.Testing.Common.Builders;

using BackendTemplate.Domain.TodoItems;

public sealed class TodoItemBuilder
{
    private string _title = "Default Title";
    private string? _description;
    private TodoStatus _status = TodoStatus.Pending;

    public TodoItemBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TodoItemBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public TodoItemBuilder WithStatus(TodoStatus status)
    {
        _status = status;
        return this;
    }

    public TodoItem Build()
    {
        var item = TodoItem.Create(_title, _description);
        if (_status != TodoStatus.Pending)
            item.UpdateStatus(_status);
        return item;
    }

    public static TodoItemBuilder InProgress() =>
        new TodoItemBuilder().WithStatus(TodoStatus.InProgress);

    public static TodoItemBuilder Done() =>
        new TodoItemBuilder().WithStatus(TodoStatus.Done);
}
```

- [ ] **Step 3: Build and run all tests**

```powershell
dotnet test backend/BackendTemplate.sln
```

Expected: `Passed! - Failed: 0, Passed: 22, Skipped: 0`

- [ ] **Step 4: Commit**

```powershell
git add backend/tests/BackendTemplate.Testing.Common/Builders/TodoItemBuilder.cs
git commit -m "feat: add TodoItemBuilder with InProgress and Done recipes"
```

---

## Exit condition

Run:

```powershell
dotnet test backend/BackendTemplate.sln
```

Expected output:
```
Passed! - Failed: 0, Passed: 22, Skipped: 0, Total: 22
```

Solution builds cleanly with zero warnings. Domain layer has no NuGet dependencies. Plan 2 (Application layer) can now begin.
