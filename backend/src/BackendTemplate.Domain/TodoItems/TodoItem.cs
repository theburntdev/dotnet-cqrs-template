namespace BackendTemplate.Domain.TodoItems;

using BackendTemplate.Domain.Common;

public sealed class TodoItem
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private TodoItem() { } // EF Core materialisation

    private TodoItem(TodoItemId id, string title, string? description)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = TodoStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public TodoItemId Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TodoStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public static TodoItem Create(string title, string? description = null) =>
        new(new TodoItemId(Guid.NewGuid()), title, description);

    public void UpdateTitle(string title) => Title = title;

    public void UpdateDescription(string? description) => Description = description;

    public void UpdateStatus(TodoStatus newStatus)
    {
        Status = newStatus;
        CompletedAtUtc = newStatus == TodoStatus.Done ? DateTime.UtcNow : null;
    }
}
