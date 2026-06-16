using TaskApi.DTOs;

namespace TaskApi.Services;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request);
    Task<(IEnumerable<TaskResponse> Items, int Total)> GetAllAsync(
        string? status,
        string? priority,
        string? search,
        int page,
        int limit);
    Task<TaskResponse?> GetByIdAsync(string id);
    Task<TaskResponse?> UpdateAsync(string id, UpdateTaskRequest request);
    Task<bool> DeleteAsync(string id);
}