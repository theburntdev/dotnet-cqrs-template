using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

namespace BackendTemplate.Application.TodoItems.CreateTodoItem;

public record CreateTodoItemCommand(string Title) : IRequest<Result<TodoItem>>;
