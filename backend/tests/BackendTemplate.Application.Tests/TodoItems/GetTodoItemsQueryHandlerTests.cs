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
