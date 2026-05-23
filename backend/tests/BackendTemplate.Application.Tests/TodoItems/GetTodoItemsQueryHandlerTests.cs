using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.GetTodoItems;
using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Testing.Common.TodoItems;
using NSubstitute;

namespace BackendTemplate.Application.Tests.TodoItems;

public class GetTodoItemsQueryHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly GetTodoItemsQueryHandler _sut;

    public GetTodoItemsQueryHandlerTests()
    {
        _sut = new GetTodoItemsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_GivenNoStatusFilter_ThenCallsGetAllAsync()
    {
        var items = new List<TodoItem> { new TodoItemBuilder().Build() };
        _repository.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(new Page<TodoItem>(items, 1, 1, 20));

        var result = await _sut.Handle(new GetTodoItemsQuery(1, 20, null), CancellationToken.None);

        Assert.Single(result.Items);
        await _repository.Received(1).GetAllAsync(1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenStatusFilter_ThenCallsGetByStatusAsync()
    {
        var items = new List<TodoItem> { TodoItemBuilder.Done().Build() };
        _repository.GetByStatusAsync(TodoStatus.Done, 1, 20, Arg.Any<CancellationToken>())
            .Returns(new Page<TodoItem>(items, 1, 1, 20));

        var result = await _sut.Handle(
            new GetTodoItemsQuery(1, 20, TodoStatus.Done), CancellationToken.None);

        Assert.Single(result.Items);
        await _repository.Received(1).GetByStatusAsync(
            TodoStatus.Done, 1, 20, Arg.Any<CancellationToken>());
    }
}
