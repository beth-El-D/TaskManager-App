using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.API.Services;
using System.Security.Claims;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly TaskService _taskService;

    public TaskController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        try
        {
            var userId = GetCurrentUserId();
            var tasks = await _taskService.GetUserTasksAsync(userId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var userId = GetCurrentUserId();
            var stats = await _taskService.GetDashboardStatsAsync(userId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.CreateTaskAsync(userId, dto);
            return Ok(task);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.UpdateTaskAsync(userId, id, dto);
            
            if (task == null)
                return NotFound("Task not found");

            return Ok(task);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _taskService.DeleteTaskAsync(userId, id);
            
            if (!success)
                return NotFound("Task not found");

            return Ok("Task deleted successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        return userId;
    }
}