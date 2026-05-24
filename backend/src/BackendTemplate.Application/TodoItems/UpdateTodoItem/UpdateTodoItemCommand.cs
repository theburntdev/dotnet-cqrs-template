namespace BackendTemplate.Application.TodoItems.UpdateTodoItem;

using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public record UpdateTodoItemCommand(
    TodoItemId Id,
    string? Title,
    string? Description,
    TodoStatus? Status) : IRequest<Result<TodoItem>>;
