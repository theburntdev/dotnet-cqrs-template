using BackendTemplate.Domain.TodoItems;

namespace BackendTemplate.Api.TodoItems;

public record CreateTodoItemRequest(string Title);
public record UpdateTodoItemRequest(string? Title, TodoStatus? Status);
