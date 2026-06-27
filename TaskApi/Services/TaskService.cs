using TaskApi.DTOs;
using TaskApi.Models;
using TaskApi.Repositories;

namespace TaskApi.Services;

public class TaskServices : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskServices(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request)
    {
        var task = new TaskItem
        {
            Id = $"task_{Guid.NewGuid():N}",
            Title = request.Title!.Trim(),
            Description = request.Description?.Trim(),
            Status = "todo",
            Priority = request.Priority?.Trim().ToLower()??"medium",
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(task);
        return MapToResponse(created);
    }

    public async Task<(IEnumerable<TaskResponse> Items, int Total)> GetAllAsync(
        string? status,
        string? priority,
        string? search,
        int page,
        int limit)
    {
        var (items, total) = await _repository.GetAllAsync(status, priority, search, page, limit);
        return (items.Select(MapToResponse), total);
    }

    public async Task<TaskResponse?> GetByIdAsync(string id)
    {
        var task = await _repository.GetByIdAsync(id);
        return task is null ? null : MapToResponse(task);
    }

    public async Task<TaskResponse?> UpdateAsync(string id, UpdateTaskRequest request)
    {
        var task = await _repository.GetByIdAsync(id);
        if (task is null) return null;

        if (request.Title is not null)
            task.Title = request.Title.Trim();

        if (request.Description is not null)  
            task.Description = request.Description.Trim();

        if (request.Status is not null)
            task.Status = request.Status.Trim().ToLower();

        if (request.Priority is not null)
            task.Priority = request.Priority.Trim().ToLower();

        if (request.DueDate is not null)
            task.DueDate = request.DueDate;

        task.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(task);
        return updated is null ? null : MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static TaskResponse MapToResponse(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        Priority = task.Priority,
        DueDate = task.DueDate,
        CreatedAt = task.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        UpdatedAt = task.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
    };
}