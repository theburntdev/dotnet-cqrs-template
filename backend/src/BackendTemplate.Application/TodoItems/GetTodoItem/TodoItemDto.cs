namespace BackendTemplate.Application.TodoItems.GetTodoItem;

using BackendTemplate.Domain.TodoItems;

public record TodoItemDto(
    Guid Id,
    string Title,
    string? Description,
    TodoStatus Status,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
