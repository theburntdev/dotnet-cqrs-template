namespace BackendTemplate.Api.Models;

using BackendTemplate.Domain.TodoItems;

public record TodoItemResponse(
    Guid Id,
    string Title,
    string? Description,
    TodoStatus Status,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
