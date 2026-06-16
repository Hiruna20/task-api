using TaskApi.Models;

namespace TaskApi.Repositories;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(string id);
    Task<(IEnumerable<TaskItem> Items, int Total)> GetAllAsync(
        string? status,
        string? priority,
        string? search,
        int page,
        int limit
    );
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem?> UpdateAsync(TaskItem task);
    Task<bool> DeleteAsync(string id);
}