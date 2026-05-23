namespace BackendTemplate.Api.Mappers;

using BackendTemplate.Api.Models;
using BackendTemplate.Domain.TodoItems;

public interface ITodoItemMapper
{
    TodoItemResponse Map(TodoItem item);
}
