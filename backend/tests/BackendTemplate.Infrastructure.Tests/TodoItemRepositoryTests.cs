namespace BackendTemplate.Infrastructure.Tests;

using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Infrastructure.Repositories;
using BackendTemplate.Testing.Common.Builders;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class TodoItemRepositoryTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly AppDbContext _context;
    private IDbContextTransaction _transaction = null!;
    private readonly TodoItemRepository _repository;

    public TodoItemRepositoryTests(DatabaseFixture fixture)
    {
        _context = fixture.CreateDbContext();
        _repository = new TodoItemRepository(_context);
    }

    public async Task InitializeAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_GivenNewItem_ThenItemCanBeRetrievedById()
    {
        var item = new TodoItemBuilder().WithTitle("Buy milk").Build();
        await _repository.AddAsync(item);
        await _context.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(item.Id);

        Assert.NotNull(found);
        Assert.Equal("Buy milk", found.Title);
    }

    [Fact]
    public async Task GetAllAsync_GivenMultipleItems_ThenReturnsPagedResult()
    {
        var item1 = new TodoItemBuilder().WithTitle("First").Build();
        var item2 = new TodoItemBuilder().WithTitle("Second").Build();
        await _repository.AddAsync(item1);
        await _repository.AddAsync(item2);
        await _context.SaveChangesAsync();

        var page = await _repository.GetAllAsync(1, 10);

        Assert.Equal(2, page.Total);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task GetByStatusAsync_GivenMixedStatuses_ThenFiltersCorrectly()
    {
        var pending = new TodoItemBuilder().WithStatus(TodoStatus.Pending).Build();
        var inProgress = TodoItemBuilder.InProgress().Build();
        await _repository.AddAsync(pending);
        await _repository.AddAsync(inProgress);
        await _context.SaveChangesAsync();

        var page = await _repository.GetByStatusAsync(TodoStatus.InProgress, 1, 10);

        Assert.Equal(1, page.Total);
        Assert.Equal(TodoStatus.InProgress, page.Items[0].Status);
    }

    [Fact]
    public async Task Update_GivenModifiedItem_ThenChangesArePersisted()
    {
        var item = new TodoItemBuilder().WithTitle("Original").Build();
        await _repository.AddAsync(item);
        await _context.SaveChangesAsync();

        item.UpdateTitle("Updated");
        _repository.Update(item);
        await _context.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(item.Id);

        Assert.NotNull(found);
        Assert.Equal("Updated", found.Title);
    }

    [Fact]
    public async Task Delete_GivenExistingItem_ThenItemNotFoundAfterSave()
    {
        var item = new TodoItemBuilder().Build();
        await _repository.AddAsync(item);
        await _context.SaveChangesAsync();

        _repository.Delete(item);
        await _context.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(item.Id);

        Assert.Null(found);
    }
}
