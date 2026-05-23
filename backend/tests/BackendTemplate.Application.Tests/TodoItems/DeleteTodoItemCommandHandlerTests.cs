namespace BackendTemplate.Application.Tests.TodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.DeleteTodoItem;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Testing.Common.Builders;

public sealed class DeleteTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_GivenExistingId_ThenReturnsSuccess()
    {
        var item = new TodoItemBuilder().Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        var handler = new DeleteTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(new DeleteTodoItemCommand(item.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_GivenNonExistentId_ThenReturnsNotFoundResult()
    {
        var id = new TodoItemId(Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TodoItem?)null);
        var handler = new DeleteTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(new DeleteTodoItemCommand(id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }
}
