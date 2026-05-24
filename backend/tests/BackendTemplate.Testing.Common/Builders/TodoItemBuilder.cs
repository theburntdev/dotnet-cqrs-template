namespace BackendTemplate.Testing.Common.Builders;

using BackendTemplate.Domain.TodoItems;

public sealed class TodoItemBuilder
{
    private string _title = "Default Title";
    private string? _description;
    private TodoStatus _status = TodoStatus.Pending;

    public TodoItemBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TodoItemBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public TodoItemBuilder WithStatus(TodoStatus status)
    {
        _status = status;
        return this;
    }

    public TodoItem Build()
    {
        var item = TodoItem.Create(_title, _description);
        if (_status != TodoStatus.Pending)
            item.UpdateStatus(_status);
        return item;
    }

    public static TodoItemBuilder InProgress() =>
        new TodoItemBuilder().WithStatus(TodoStatus.InProgress);

    public static TodoItemBuilder Done() =>
        new TodoItemBuilder().WithStatus(TodoStatus.Done);
}
