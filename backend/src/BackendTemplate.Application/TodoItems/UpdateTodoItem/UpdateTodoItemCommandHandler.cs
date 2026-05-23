using BackendTemplate.Application.Common;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

namespace BackendTemplate.Application.TodoItems.UpdateTodoItem;

public sealed class UpdateTodoItemCommandHandler(
    ITodoItemRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTodoItemCommand, Result<TodoItem>>
{
    public async Task<Result<TodoItem>> Handle(UpdateTodoItemCommand request, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(new TodoItemId(request.Id), ct);
        if (item is null)
            return Result<TodoItem>.Failure("TodoItem not found.", ErrorKind.NotFound);

        if (request.Title is not null)
            item.UpdateTitle(request.Title);
        if (request.Status.HasValue)
            item.UpdateStatus(request.Status.Value);

        repository.Update(item);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<TodoItem>.Success(item);
    }
}
