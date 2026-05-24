namespace BackendTemplate.Api.Mappers;

using BackendTemplate.Api.Models;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class TodoItemMapper
{
    public static partial TodoItemResponse Map(TodoItem item);

    private static Guid MapId(TodoItemId id) => id.Value;
}
