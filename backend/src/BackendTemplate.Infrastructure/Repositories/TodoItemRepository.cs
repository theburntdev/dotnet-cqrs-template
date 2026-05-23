namespace BackendTemplate.Infrastructure.Repositories;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using Microsoft.EntityFrameworkCore;

public sealed class TodoItemRepository : ITodoItemRepository
{
    private readonly AppDbContext _context;

    public TodoItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TodoItem?> GetByIdAsync(TodoItemId id, CancellationToken ct = default)
        => await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Page<TodoItem>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var total = await _context.TodoItems.CountAsync(ct);
        var items = await _context.TodoItems
            .OrderBy(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new Page<TodoItem>(items, total, page, pageSize);
    }

    public async Task<Page<TodoItem>> GetByStatusAsync(
        TodoStatus status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.TodoItems.Where(x => x.Status == status);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new Page<TodoItem>(items, total, page, pageSize);
    }

    public async Task AddAsync(TodoItem entity, CancellationToken ct = default)
        => await _context.TodoItems.AddAsync(entity, ct);

    public void Update(TodoItem entity)
        => _context.TodoItems.Update(entity);

    public void Delete(TodoItem entity)
        => _context.TodoItems.Remove(entity);
}
