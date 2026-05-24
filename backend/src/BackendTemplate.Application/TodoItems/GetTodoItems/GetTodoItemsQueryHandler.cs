namespace BackendTemplate.Application.TodoItems.GetTodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems.GetTodoItem;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public sealed class GetTodoItemsQueryHandler : IRequestHandler<GetTodoItemsQuery, Page<TodoItemDto>>
{
    private readonly ITodoItemRepository _repository;

    public GetTodoItemsQueryHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<Page<TodoItemDto>> Handle(GetTodoItemsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Status is not null
            ? await _repository.GetByStatusAsync(request.Status.Value, request.Page, request.PageSize, cancellationToken)
            : await _repository.GetAllAsync(request.Page, request.PageSize, cancellationToken);

        return new Page<TodoItemDto>(
            page.Items.Select(MapToDto).ToList(),
            page.Total,
            page.PageNumber,
            page.PageSize);
    }

    private static TodoItemDto MapToDto(TodoItem item) =>
        new(item.Id.Value, item.Title, item.Description, item.Status, item.CreatedAtUtc, item.CompletedAtUtc);
}
