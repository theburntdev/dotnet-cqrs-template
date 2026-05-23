namespace BackendTemplate.Api.Mappers;

using BackendTemplate.Api.Models;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using Riok.Mapperly.Abstractions;

[Mapper]
public partial class TodoItemMapper : ITodoItemMapper
{
    public partial TodoItemResponse Map(TodoItem item);

    private static Guid MapId(TodoItemId id) => id.Value;
}
