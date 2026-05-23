using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.TodoItems;
using MediatR;

namespace BackendTemplate.Application.TodoItems.GetTodoItems;

public record GetTodoItemsQuery(int Page = 1, int PageSize = 20, TodoStatus? Status = null)
    : IRequest<Page<TodoItemResult>>;
