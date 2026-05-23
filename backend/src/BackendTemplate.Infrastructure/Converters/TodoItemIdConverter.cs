namespace BackendTemplate.Infrastructure.Converters;

using BackendTemplate.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public sealed class TodoItemIdConverter : ValueConverter<TodoItemId, Guid>
{
    public TodoItemIdConverter()
        : base(id => id.Value, value => new TodoItemId(value)) { }
}
