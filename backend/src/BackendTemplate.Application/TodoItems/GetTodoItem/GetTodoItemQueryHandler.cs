using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

namespace BackendTemplate.Application.TodoItems.GetTodoItem;

public sealed class GetTodoItemQueryHandler(ITodoItemRepository repository)
    : IRequestHandler<GetTodoItemQuery, Result<TodoItemResult>>
{
    public async Task<Result<TodoItemResult>> Handle(GetTodoItemQuery request, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(new TodoItemId(request.Id), ct);
        if (item is null)
            return Result<TodoItemResult>.Failure("TodoItem not found.", ErrorKind.NotFound);

        return Result<TodoItemResult>.Success(
            new TodoItemResult(item.Id.Value, item.Title, item.Status, item.CreatedAtUtc));
    }
}
