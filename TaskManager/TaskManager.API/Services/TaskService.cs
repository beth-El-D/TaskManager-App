using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Data;
using TaskManager.Domain.Entities;
using TaskManager.API.DTOs;

namespace TaskManager.API.Services;

public class TaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskResponseDto>> GetUserTasksAsync(Guid userId)
    {
        var tasks = await _context.Tasks
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        return tasks.Select(MapToResponseDto).ToList();
    }

    public async Task<TaskResponseDto> CreateTaskAsync(Guid userId, CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> UpdateTaskAsync(Guid userId, Guid taskId, UpdateTaskDto dto)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null) return null;

        if (!string.IsNullOrEmpty(dto.Title))
            task.Title = dto.Title;

        if (dto.Description != null)
            task.Description = dto.Description;

        if (dto.Priority.HasValue)
            task.Priority = dto.Priority.Value;

        if (dto.Status.HasValue)
            task.Status = dto.Status.Value;

        if (dto.DueDate.HasValue)
            task.DueDate = dto.DueDate.Value;

        if (dto.IsCompleted.HasValue)
        {
            task.IsCompleted = dto.IsCompleted.Value;
            if (dto.IsCompleted.Value && task.CompletedAt == null)
                task.CompletedAt = DateTime.UtcNow;
            else if (!dto.IsCompleted.Value)
                task.CompletedAt = null;
        }

        await _context.SaveChangesAsync();

        return MapToResponseDto(task);
    }

    public async Task<bool> DeleteTaskAsync(Guid userId, Guid taskId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var allTasks = await _context.Tasks
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var overdueTasks = allTasks.Where(t => t.DueDate < today && !t.IsCompleted).ToList();
        var todayTasks = allTasks.Where(t => t.DueDate >= today && t.DueDate < tomorrow).ToList();
        var upcomingTasks = allTasks.Where(t => t.DueDate >= tomorrow && !t.IsCompleted).ToList();
        var completedToday = allTasks.Where(t => t.CompletedAt?.Date == today).Count();

        return new DashboardStatsDto
        {
            OverdueTasks = overdueTasks.Select(MapToResponseDto).ToList(),
            TodayTasks = todayTasks.Select(MapToResponseDto).ToList(),
            UpcomingTasks = upcomingTasks.Take(5).Select(MapToResponseDto).ToList(),
            TotalTasksToday = todayTasks.Count,
            CompletedToday = completedToday,
            OverdueCount = overdueTasks.Count
        };
    }

    private TaskResponseDto MapToResponseDto(TaskItem task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority,
            Status = task.Status,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            CompletedAt = task.CompletedAt,
            UserId = task.UserId
        };
    }
}

public class DashboardStatsDto
{
    public List<TaskResponseDto> OverdueTasks { get; set; } = new();
    public List<TaskResponseDto> TodayTasks { get; set; } = new();
    public List<TaskResponseDto> UpcomingTasks { get; set; } = new();
    public int TotalTasksToday { get; set; }
    public int CompletedToday { get; set; }
    public int OverdueCount { get; set; }
}