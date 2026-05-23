using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.TodoItems;
using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Infrastructure.Persistence.Repositories;

public sealed class TodoItemRepository(AppDbContext context) : ITodoItemRepository
{
    public async Task<TodoItem?> GetByIdAsync(TodoItemId id, CancellationToken ct = default) =>
        await context.TodoItems.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Page<TodoItem>> GetAllAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var total = await context.TodoItems.CountAsync(ct);
        var items = await context.TodoItems
            .OrderBy(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new Page<TodoItem>(items, total, page, pageSize);
    }

    public async Task<Page<TodoItem>> GetByStatusAsync(
        TodoStatus status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.TodoItems.Where(x => x.Status == status);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new Page<TodoItem>(items, total, page, pageSize);
    }

    public async Task AddAsync(TodoItem entity, CancellationToken ct = default) =>
        await context.TodoItems.AddAsync(entity, ct);

    public void Update(TodoItem entity) => context.TodoItems.Update(entity);

    public void Delete(TodoItem entity) => context.TodoItems.Remove(entity);
}
