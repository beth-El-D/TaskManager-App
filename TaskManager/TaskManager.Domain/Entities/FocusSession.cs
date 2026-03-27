namespace TaskManager.Domain.Entities;

public class FocusSession
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int PlannedDurationMinutes { get; set; } = 25; // Default Pomodoro
    public int? ActualDurationMinutes { get; set; }
    public bool IsCompleted { get; set; } = false;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign Keys
    public Guid UserId { get; set; }
    public Guid? TaskId { get; set; }
    
    // Navigation Properties
    public User User { get; set; } = default!;
    public TaskItem? Task { get; set; }
}