using TaskApi.DTOs;
using TaskApi.Models;

namespace TaskApi.Validation;

public static class TaskValidator
{
    private static readonly string[] ValidStatuses = ["todo", "in_progress", "done"];
    private static readonly string[] ValidPriorities = ["low", "medium", "high"];

    public static List<ErrorDetail> ValidateCreate(CreateTaskRequest request)
    {
        var errors = new List<ErrorDetail>();

        // Title
        if (string.IsNullOrWhiteSpace(request.Title))
            errors.Add(new ErrorDetail("title", "Title is required."));
        else if (request.Title.Trim().Length > 120)
            errors.Add(new ErrorDetail("title", "Title must be 120 characters or fewer."));

        // Description
        if (request.Description is not null && request.Description.Trim().Length > 1000)
            errors.Add(new ErrorDetail("description", "Description must be 1000 characters or fewer."));

        // Priority
        if (request.Priority is not null && !ValidPriorities.Contains(request.Priority.ToLower()))
            errors.Add(new ErrorDetail("priority", $"Priority must be one of: {string.Join(", ", ValidPriorities)}."));

        // DueDate
        if (request.DueDate is not null && !DateOnly.TryParse(request.DueDate, out _))
            errors.Add(new ErrorDetail("dueDate", "DueDate must be a valid ISO 8601 date (e.g. 2026-06-30)."));

        return errors;
    }

    public static List<ErrorDetail> ValidateUpdate(UpdateTaskRequest request)
    {
        var errors = new List<ErrorDetail>();

        // Reject empty update body
        if (request.Title is null &&
            request.Description is null &&
            request.Status is null &&
            request.Priority is null &&
            request.DueDate is null)
        {
            errors.Add(new ErrorDetail("body", "At least one field must be provided for update."));
            return errors;
        }

        // Title
        if (request.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                errors.Add(new ErrorDetail("title", "Title cannot be empty."));
            else if (request.Title.Trim().Length > 120)
                errors.Add(new ErrorDetail("title", "Title must be 120 characters or fewer."));
        }

        // Description
        if (request.Description is not null && request.Description.Trim().Length > 1000)
            errors.Add(new ErrorDetail("description", "Description must be 1000 characters or fewer."));

        // Status
        if (request.Status is not null && !ValidStatuses.Contains(request.Status.ToLower()))
            errors.Add(new ErrorDetail("status", $"Status must be one of: {string.Join(", ", ValidStatuses)}."));

        // Priority
        if (request.Priority is not null && !ValidPriorities.Contains(request.Priority.ToLower()))
            errors.Add(new ErrorDetail("priority", $"Priority must be one of: {string.Join(", ", ValidPriorities)}."));

        // DueDate
        if (request.DueDate is not null && !DateOnly.TryParse(request.DueDate, out _))
            errors.Add(new ErrorDetail("dueDate", "DueDate must be a valid ISO 8601 date (e.g. 2026-06-30)."));

        return errors;
    }

    public static List<ErrorDetail> ValidateListParams(string? status, string? priority)
    {
        var errors = new List<ErrorDetail>();

        if (status is not null && !ValidStatuses.Contains(status.ToLower()))
            errors.Add(new ErrorDetail("status", $"Status must be one of: {string.Join(", ", ValidStatuses)}."));

        if (priority is not null && !ValidPriorities.Contains(priority.ToLower()))
            errors.Add(new ErrorDetail("priority", $"Priority must be one of: {string.Join(", ", ValidPriorities)}."));

        return errors;
    }
}