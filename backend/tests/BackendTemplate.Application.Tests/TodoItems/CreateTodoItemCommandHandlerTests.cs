using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Application.TodoItems.CreateTodoItem;
using BackendTemplate.Domain.TodoItems;
using NSubstitute;

namespace BackendTemplate.Application.Tests.TodoItems;

public class CreateTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository = Substitute.For<ITodoItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateTodoItemCommandHandler _sut;

    public CreateTodoItemCommandHandlerTests()
    {
        _sut = new CreateTodoItemCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ThenReturnsCreatedTodoItem()
    {
        var command = new CreateTodoItemCommand("Buy milk");

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Buy milk", result.Value.Title);
        Assert.Equal(TodoStatus.Pending, result.Value.Status);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ThenPersistsItemAndSavesChanges()
    {
        var command = new CreateTodoItemCommand("Buy milk");

        await _sut.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<TodoItem>(t => t.Title == "Buy milk"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
