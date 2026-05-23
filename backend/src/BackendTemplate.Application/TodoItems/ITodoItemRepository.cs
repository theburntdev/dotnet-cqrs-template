namespace BackendTemplate.Application.TodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;

public interface ITodoItemRepository : IRepository<TodoItem, TodoItemId>
{
    Task<Page<TodoItem>> GetByStatusAsync(
        TodoStatus status,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
