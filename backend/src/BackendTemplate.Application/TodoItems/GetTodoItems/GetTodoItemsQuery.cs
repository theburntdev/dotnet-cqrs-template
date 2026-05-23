namespace BackendTemplate.Application.TodoItems.GetTodoItems;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems.GetTodoItem;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public record GetTodoItemsQuery(int Page, int PageSize, TodoStatus? Status)
    : IRequest<Page<TodoItemDto>>;
