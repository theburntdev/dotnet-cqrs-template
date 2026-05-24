namespace BackendTemplate.Application.Tests.TodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.UpdateTodoItem;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Testing.Common.Builders;

public sealed class UpdateTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_GivenNonExistentId_ThenReturnsNotFoundResult()
    {
        var id = new TodoItemId(Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TodoItem?)null);
        var handler = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(
            new UpdateTodoItemCommand(id, "New title", null, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Handle_GivenNullTitle_ThenTitleUnchanged()
    {
        var item = new TodoItemBuilder().WithTitle("Original").Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(
            new UpdateTodoItemCommand(item.Id, null, null, null),
            CancellationToken.None);

        Assert.Equal("Original", result.Value.Title);
    }

    [Fact]
    public async Task Handle_GivenNewTitle_ThenTitleUpdated()
    {
        var item = new TodoItemBuilder().WithTitle("Original").Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(
            new UpdateTodoItemCommand(item.Id, "Updated", null, null),
            CancellationToken.None);

        Assert.Equal("Updated", result.Value.Title);
    }

    [Fact]
    public async Task Handle_GivenEmptyDescription_ThenDescriptionCleared()
    {
        var item = new TodoItemBuilder().WithDescription("Some description").Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(
            new UpdateTodoItemCommand(item.Id, null, "", null),
            CancellationToken.None);

        Assert.Null(result.Value.Description);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ThenCallsSaveChanges()
    {
        var item = new TodoItemBuilder().Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new UpdateTodoItemCommandHandler(_repository, _unitOfWork);

        await handler.Handle(
            new UpdateTodoItemCommand(item.Id, "New title", null, null),
            CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
