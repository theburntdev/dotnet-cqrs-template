using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Infrastructure.Persistence.Repositories;
using BackendTemplate.Infrastructure.Tests.Common;
using BackendTemplate.Testing.Common.TodoItems;

namespace BackendTemplate.Infrastructure.Tests.TodoItems;

public class TodoItemRepositoryTests(DatabaseFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task AddAsync_GivenNewItem_ThenPersistsToDatabase()
    {
        var repository = new TodoItemRepository(DbContext);
        var item = new TodoItemBuilder().WithTitle("Test item").Build();

        await repository.AddAsync(item);
        await DbContext.SaveChangesAsync();

        var found = await repository.GetByIdAsync(item.Id);
        Assert.NotNull(found);
        Assert.Equal("Test item", found.Title);
    }

    [Fact]
    public async Task GetAllAsync_GivenMultipleItems_ThenReturnsPaged()
    {
        var repository = new TodoItemRepository(DbContext);
        await repository.AddAsync(new TodoItemBuilder().WithTitle("A").Build());
        await repository.AddAsync(new TodoItemBuilder().WithTitle("B").Build());
        await repository.AddAsync(new TodoItemBuilder().WithTitle("C").Build());
        await DbContext.SaveChangesAsync();

        var page = await repository.GetAllAsync(1, 2);

        Assert.Equal(2, page.Items.Count);
        Assert.Equal(3, page.Total);
    }

    [Fact]
    public async Task GetByStatusAsync_GivenMixedStatuses_ThenFiltersCorrectly()
    {
        var repository = new TodoItemRepository(DbContext);
        await repository.AddAsync(new TodoItemBuilder().Build());
        await repository.AddAsync(TodoItemBuilder.Done().Build());
        await DbContext.SaveChangesAsync();

        var result = await repository.GetByStatusAsync(TodoStatus.Pending, 1, 20);

        Assert.All(result.Items, i => Assert.Equal(TodoStatus.Pending, i.Status));
    }

    [Fact]
    public async Task Delete_GivenExistingItem_ThenRemovesFromDatabase()
    {
        var repository = new TodoItemRepository(DbContext);
        var item = new TodoItemBuilder().Build();
        await repository.AddAsync(item);
        await DbContext.SaveChangesAsync();

        repository.Delete(item);
        await DbContext.SaveChangesAsync();

        var found = await repository.GetByIdAsync(item.Id);
        Assert.Null(found);
    }
}
