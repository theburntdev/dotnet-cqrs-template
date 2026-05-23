using BackendTemplate.Application.Common;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

namespace BackendTemplate.Application.TodoItems.CreateTodoItem;

public sealed class CreateTodoItemCommandHandler(
    ITodoItemRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTodoItemCommand, Result<TodoItem>>
{
    public async Task<Result<TodoItem>> Handle(CreateTodoItemCommand request, CancellationToken ct)
    {
        var item = TodoItem.Create(request.Title);
        await repository.AddAsync(item, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<TodoItem>.Success(item);
    }
}
