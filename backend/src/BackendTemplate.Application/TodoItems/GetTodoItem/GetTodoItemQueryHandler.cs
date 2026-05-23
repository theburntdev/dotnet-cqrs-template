namespace BackendTemplate.Application.TodoItems.GetTodoItem;

using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public sealed class GetTodoItemQueryHandler : IRequestHandler<GetTodoItemQuery, Result<TodoItemDto>>
{
    private readonly ITodoItemRepository _repository;

    public GetTodoItemQueryHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TodoItemDto>> Handle(GetTodoItemQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            return Result<TodoItemDto>.Failure($"Todo item '{request.Id.Value}' not found.", ErrorKind.NotFound);

        return Result<TodoItemDto>.Success(MapToDto(item));
    }

    private static TodoItemDto MapToDto(TodoItem item) =>
        new(item.Id.Value, item.Title, item.Description, item.Status, item.CreatedAtUtc, item.CompletedAtUtc);
}
