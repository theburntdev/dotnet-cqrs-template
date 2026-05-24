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
