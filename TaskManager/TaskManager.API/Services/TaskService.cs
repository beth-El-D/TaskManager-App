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
            .Include(t => t.Project)
            .Include(t => t.Category)
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
            ProjectId = dto.ProjectId,
            CategoryId = dto.CategoryId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(task)
            .Reference(t => t.Project)
            .LoadAsync();
        await _context.Entry(task)
            .Reference(t => t.Category)
            .LoadAsync();

        return MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> UpdateTaskAsync(Guid userId, Guid taskId, UpdateTaskDto dto)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Category)
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

        if (dto.ProjectId.HasValue)
            task.ProjectId = dto.ProjectId.Value;

        if (dto.CategoryId.HasValue)
            task.CategoryId = dto.CategoryId.Value;

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
            .Include(t => t.Project)
            .Include(t => t.Category)
            .ToListAsync();

        var overdueTasks = allTasks.Where(t => t.DueDate < today && !t.IsCompleted).ToList();
        var todayTasks = allTasks.Where(t => t.DueDate >= today && t.DueDate < tomorrow).ToList();
        var upcomingTasks = allTasks.Where(t => t.DueDate >= tomorrow && !t.IsCompleted).ToList();
        var completedToday = allTasks.Where(t => t.CompletedAt?.Date == today).Count();

        // Calculate weekly progress
        var weeklyProgress = GetWeeklyProgress(allTasks);

        // Calculate task completion trend
        var lastWeekCompleted = allTasks.Where(t => t.CompletedAt?.Date >= today.AddDays(-7) && t.CompletedAt?.Date < today).Count();
        var thisWeekCompleted = allTasks.Where(t => t.CompletedAt?.Date >= today.AddDays(-7)).Count();
        var completionTrend = lastWeekCompleted > 0 ? ((thisWeekCompleted - lastWeekCompleted) * 100 / lastWeekCompleted) : 0;

        return new DashboardStatsDto
        {
            OverdueTasks = overdueTasks.Select(MapToResponseDto).ToList(),
            TodayTasks = todayTasks.Select(MapToResponseDto).ToList(),
            UpcomingTasks = upcomingTasks.Take(5).Select(MapToResponseDto).ToList(),
            TotalTasksToday = todayTasks.Count,
            CompletedToday = completedToday,
            OverdueCount = overdueTasks.Count,
            WeeklyProgress = weeklyProgress,
            CompletionTrend = completionTrend
        };
    }

    private List<WeeklyProgressDto> GetWeeklyProgress(List<TaskItem> allTasks)
    {
        var today = DateTime.Today;
        var weeklyProgress = new List<WeeklyProgressDto>();

        // Get last 7 days
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayTasks = allTasks.Where(t => t.DueDate.Date == date).ToList();
            var completedTasks = dayTasks.Where(t => t.IsCompleted).Count();
            var totalTasks = dayTasks.Count;
            var completionRate = totalTasks > 0 ? (completedTasks * 100 / totalTasks) : 0;

            weeklyProgress.Add(new WeeklyProgressDto
            {
                Date = date,
                DayName = date.ToString("ddd").Substring(0, 1), // M, T, W, etc.
                CompletionRate = completionRate,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                IsToday = date == today
            });
        }

        return weeklyProgress;
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
            UserId = task.UserId,
            ProjectId = task.ProjectId,
            CategoryId = task.CategoryId,
            ProjectName = task.Project?.Name,
            CategoryName = task.Category?.Name,
            ProjectColor = task.Project?.Color,
            CategoryColor = task.Category?.Color
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
    public List<WeeklyProgressDto> WeeklyProgress { get; set; } = new();
    public int CompletionTrend { get; set; }
}

public class WeeklyProgressDto
{
    public DateTime Date { get; set; }
    public string DayName { get; set; } = string.Empty;
    public int CompletionRate { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public bool IsToday { get; set; }
}