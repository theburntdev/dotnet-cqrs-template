using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.UpdateTodoItem;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Testing.Common.TodoItems;
using NSubstitute;

namespace BackendTemplate.Application.Tests.TodoItems;

public class UpdateTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateTodoItemCommandHandler _sut;

    public UpdateTodoItemCommandHandlerTests()
    {
        _sut = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_GivenExistingItem_ThenUpdatesAndReturnsSuccess()
    {
        var item = new TodoItemBuilder().WithTitle("Old title").Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _sut.Handle(
            new UpdateTodoItemCommand(item.Id.Value, "New title", TodoStatus.InProgress),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New title", result.Value.Title);
        Assert.Equal(TodoStatus.InProgress, result.Value.Status);
    }

    [Fact]
    public async Task Handle_GivenNonExistentId_ThenReturnsNotFoundFailure()
    {
        _repository.GetByIdAsync(Arg.Any<TodoItemId>(), Arg.Any<CancellationToken>())
            .Returns((TodoItem?)null);

        var result = await _sut.Handle(
            new UpdateTodoItemCommand(Guid.NewGuid(), "title", null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Handle_GivenNullTitle_ThenTitleUnchanged()
    {
        var item = new TodoItemBuilder().WithTitle("Keep this").Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _sut.Handle(
            new UpdateTodoItemCommand(item.Id.Value, null, TodoStatus.Done),
            CancellationToken.None);

        Assert.Equal("Keep this", result.Value.Title);
    }
}
