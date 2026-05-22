using BackendTemplate.Domain.TodoItems;

namespace BackendTemplate.Domain.Tests.TodoItems;

public class TodoItemTests
{
    [Fact]
    public void Create_GivenValidTitle_ThenReturnsPendingItemWithTitle()
    {
        var item = TodoItem.Create("Buy milk");

        Assert.Equal("Buy milk", item.Title);
        Assert.Equal(TodoStatus.Pending, item.Status);
        Assert.NotEqual(default, item.Id);
        Assert.True(item.CreatedAtUtc <= DateTime.UtcNow);
        Assert.True(item.CreatedAtUtc > DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public void UpdateTitle_GivenNewTitle_ThenTitleChanges()
    {
        var item = TodoItem.Create("Old");
        item.UpdateTitle("New");
        Assert.Equal("New", item.Title);
    }

    [Fact]
    public void UpdateStatus_GivenNewStatus_ThenStatusChanges()
    {
        var item = TodoItem.Create("Task");
        item.UpdateStatus(TodoStatus.InProgress);
        Assert.Equal(TodoStatus.InProgress, item.Status);
    }

    [Fact]
    public void Create_GivenTwoCalls_ThenIdsAreUnique()
    {
        var a = TodoItem.Create("A");
        var b = TodoItem.Create("B");
        Assert.NotEqual(a.Id, b.Id);
    }
}
