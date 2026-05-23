using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.TodoItems;
using MediatR;

namespace BackendTemplate.Application.TodoItems.GetTodoItems;

public sealed class GetTodoItemsQueryHandler(ITodoItemRepository repository)
    : IRequestHandler<GetTodoItemsQuery, Page<TodoItemResult>>
{
    public async Task<Page<TodoItemResult>> Handle(GetTodoItemsQuery request, CancellationToken ct)
    {
        var page = request.Status.HasValue
            ? await repository.GetByStatusAsync(request.Status.Value, request.Page, request.PageSize, ct)
            : await repository.GetAllAsync(request.Page, request.PageSize, ct);

        var items = page.Items
            .Select(i => new TodoItemResult(i.Id.Value, i.Title, i.Status, i.CreatedAtUtc))
            .ToList();

        return new Page<TodoItemResult>(items, page.Total, page.PageNumber, page.PageSize);
    }
}
