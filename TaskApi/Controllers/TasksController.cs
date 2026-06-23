using Microsoft.AspNetCore.Mvc;
using TaskApi.DTOs;
using TaskApi.Services;
using TaskApi.Validation;

namespace TaskApi.Controllers;

[ApiController]
[Route("api/v1/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // POST /api/v1/tasks
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var errors = TaskValidator.ValidateCreate(request);
        if (errors.Count > 0)
            return BadRequest(BuildValidationError(errors));

        var task = await _taskService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    // GET /api/v1/tasks
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var errors = TaskValidator.ValidateListParams(status, priority);

        if (page < 1)
            errors.Add(new ErrorDetail("page", "Page must be a positive integer."));

        if (limit < 1 || limit > 100)
            errors.Add(new ErrorDetail("limit", "Limit must be a positive integer no greater than 100."));

        if (errors.Count > 0)
            return BadRequest(BuildValidationError(errors));

        var (items, total) = await _taskService.GetAllAsync(status, priority, search, page, limit);

        return Ok(new
        {
            data = items,
            pagination = new { page, limit, total }
        });
    }

    // GET /api/v1/tasks/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task is null)
            return NotFound(BuildNotFoundError(id));

        return Ok(task);
    }

    // PATCH /api/v1/tasks/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTaskRequest request)
    {
        var errors = TaskValidator.ValidateUpdate(request);
        if (errors.Count > 0)
            return BadRequest(BuildValidationError(errors));

        var task = await _taskService.UpdateAsync(id, request);
        if (task is null)
            return NotFound(BuildNotFoundError(id));

        return Ok(task);
    }

    // DELETE /api/v1/tasks/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _taskService.DeleteAsync(id);
        if (!deleted)
            return NotFound(BuildNotFoundError(id));

        return NoContent();
    }

    // --- Helpers ---

    private static ErrorResponse BuildValidationError(List<ErrorDetail> errors) =>
        new(new ErrorBody(ErrorCodes.ValidationError, "The request body is invalid.", errors));

    private static ErrorResponse BuildNotFoundError(string id) =>
        new(new ErrorBody(ErrorCodes.NotFound, $"Task with id '{id}' was not found."));
} 