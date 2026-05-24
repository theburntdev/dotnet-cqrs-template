namespace BackendTemplate.Api.Models;

using BackendTemplate.Domain.TodoItems;

public record UpdateTodoItemRequest(
    string? Title,
    string? Description,
    TodoStatus? Status);
