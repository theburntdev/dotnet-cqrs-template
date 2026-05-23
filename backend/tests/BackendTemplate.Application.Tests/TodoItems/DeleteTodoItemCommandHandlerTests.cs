using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.DeleteTodoItem;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using BackendTemplate.Testing.Common.TodoItems;
using NSubstitute;

namespace BackendTemplate.Application.Tests.TodoItems;

public class DeleteTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteTodoItemCommandHandler _sut;

    public DeleteTodoItemCommandHandlerTests()
    {
        _sut = new DeleteTodoItemCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_GivenExistingItem_ThenDeletesAndReturnsSuccess()
    {
        var item = new TodoItemBuilder().Build();
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _sut.Handle(
            new DeleteTodoItemCommand(item.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.Received(1).Delete(item);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNonExistentId_ThenReturnsNotFoundFailure()
    {
        _repository.GetByIdAsync(Arg.Any<TodoItemId>(), Arg.Any<CancellationToken>())
            .Returns((TodoItem?)null);

        var result = await _sut.Handle(
            new DeleteTodoItemCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }
}
