using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.TodoItems;

namespace BackendTemplate.Api.TodoItems;

/// <summary>
/// Maps domain entities to read models for API responses.
/// Note: Riok.Mapperly 4.3.1 is not compatible with .NET 10 / Roslyn 5.x;
/// this mapper is implemented manually with the same semantics.
/// </summary>
public class TodoItemMapper
{
    public TodoItemResult ToResult(TodoItem item) =>
        new(item.Id.Value, item.Title, item.Status, item.CreatedAtUtc);
}
