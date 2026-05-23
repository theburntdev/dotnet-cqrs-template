using BackendTemplate.Application.Common;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

namespace BackendTemplate.Application.TodoItems.DeleteTodoItem;

public sealed class DeleteTodoItemCommandHandler(
    ITodoItemRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTodoItemCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteTodoItemCommand request, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(new TodoItemId(request.Id), ct);
        if (item is null)
            return Result<bool>.Failure("TodoItem not found.", ErrorKind.NotFound);

        repository.Delete(item);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
