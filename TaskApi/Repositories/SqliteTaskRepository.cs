using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.Models;

namespace TaskApi.Repositories;

public class SqliteTaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public SqliteTaskRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TaskItem?> GetByIdAsync(string id)
    {
        return await _db.Tasks.FindAsync(id);
    }

    public async Task<(IEnumerable<TaskItem> Items, int Total)> GetAllAsync(
        string? status,
        string? priority,
        string? search,
        int page,
        int limit)
    {
        var query = _db.Tasks.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<TaskItemStatus>(status, ignoreCase: true);
            query = query.Where(t => t.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            var priorityEnum = Enum.Parse<TaskItemPriority>(priority, ignoreCase: true);
            query = query.Where(t => t.Priority == priorityEnum);
        }

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t =>
                t.Title.Contains(search) ||
                (t.Description != null && t.Description.Contains(search)));

        query = query.OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (items, total);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem?> UpdateAsync(TaskItem task)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return false;

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return true;
    }
}