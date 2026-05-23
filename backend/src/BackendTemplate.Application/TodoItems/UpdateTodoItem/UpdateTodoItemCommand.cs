using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

namespace BackendTemplate.Application.TodoItems.UpdateTodoItem;

public record UpdateTodoItemCommand(Guid Id, string? Title, TodoStatus? Status)
    : IRequest<Result<TodoItem>>;
