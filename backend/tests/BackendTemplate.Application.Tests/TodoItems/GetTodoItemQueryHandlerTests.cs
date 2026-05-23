using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.GetTodoItem;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Testing.Common.TodoItems;
using NSubstitute;

namespace BackendTemplate.Application.Tests.TodoItems;

public class GetTodoItemQueryHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly GetTodoItemQueryHandler _sut;

    public GetTodoItemQueryHandlerTests()
    {
        _sut = new GetTodoItemQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_GivenExistingId_ThenReturnsSuccessWithResult()
    {
        var item = new TodoItemBuilder().WithTitle("Test item").Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _sut.Handle(new GetTodoItemQuery(item.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(item.Id.Value, result.Value.Id);
        Assert.Equal("Test item", result.Value.Title);
    }

    [Fact]
    public async Task Handle_GivenNonExistentId_ThenReturnsNotFoundFailure()
    {
        _repository.GetByIdAsync(Arg.Any<TodoItemId>(), Arg.Any<CancellationToken>())
            .Returns((TodoItem?)null);

        var result = await _sut.Handle(new GetTodoItemQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }
}
