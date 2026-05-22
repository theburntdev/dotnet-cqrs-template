namespace BackendTemplate.Domain.TodoItems;

public class TodoItem
{
    public TodoItemId Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public TodoStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private TodoItem() { }

    public static TodoItem Create(string title) => new()
    {
        Id = new TodoItemId(Guid.NewGuid()),
        Title = title,
        Status = TodoStatus.Pending,
        CreatedAtUtc = DateTime.UtcNow
    };

    public void UpdateTitle(string title) => Title = title;

    public void UpdateStatus(TodoStatus status) => Status = status;
}
