using BackendTemplate.Domain.TodoItems;

namespace BackendTemplate.Application.TodoItems;

public record TodoItemResult(Guid Id, string Title, TodoStatus Status, DateTime CreatedAtUtc);
