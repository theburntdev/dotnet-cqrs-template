namespace BackendTemplate.Application.Tests.TodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.CreateTodoItem;

public sealed class CreateTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_GivenValidCommand_ThenCreatesAndReturnsTodoItem()
    {
        var handler = new CreateTodoItemCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(
            new CreateTodoItemCommand("Buy milk", "From the store"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Buy milk", result.Value.Title);
        Assert.Equal("From the store", result.Value.Description);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ThenCallsSaveChanges()
    {
        var handler = new CreateTodoItemCommandHandler(_repository, _unitOfWork);

        await handler.Handle(new CreateTodoItemCommand("Buy milk", null), CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
