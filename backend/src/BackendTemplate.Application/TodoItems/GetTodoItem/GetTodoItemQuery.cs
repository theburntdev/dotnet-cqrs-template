using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.Common;
using MediatR;

namespace BackendTemplate.Application.TodoItems.GetTodoItem;

public record GetTodoItemQuery(Guid Id) : IRequest<Result<TodoItemResult>>;
