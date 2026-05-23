namespace BackendTemplate.Application.TodoItems.DeleteTodoItem;

using BackendTemplate.Domain.Common;
using MediatR;

public record DeleteTodoItemCommand(TodoItemId Id) : IRequest<Result>;
