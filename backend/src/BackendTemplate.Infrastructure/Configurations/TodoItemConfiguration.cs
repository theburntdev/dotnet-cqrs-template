namespace BackendTemplate.Infrastructure.Configurations;

using BackendTemplate.Domain.TodoItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(TodoItem.TitleMaxLength);

        builder.Property(x => x.Description)
            .HasMaxLength(TodoItem.DescriptionMaxLength);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.CompletedAtUtc)
            .HasColumnType("timestamp with time zone");
    }
}
