namespace BackendTemplate.Application.TodoItems.GetTodoItem;

using BackendTemplate.Domain.Common;
using MediatR;

public record GetTodoItemQuery(TodoItemId Id) : IRequest<Result<TodoItemDto>>;
