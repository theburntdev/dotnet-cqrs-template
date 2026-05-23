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
