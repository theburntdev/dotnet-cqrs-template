namespace BackendTemplate.Application.TodoItems.CreateTodoItem;

using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public record CreateTodoItemCommand(string Title, string? Description)
    : IRequest<Result<TodoItem>>;
