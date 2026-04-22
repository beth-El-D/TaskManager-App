namespace TaskManager.Domain.Entities;

public class FocusSession
{
    public Guid Id { get; set; }
    public string? TaskTitle { get; set; } // Optional task being focused on
    public Guid? TaskId { get; set; } // Optional link to specific task
    public int DurationMinutes { get; set; } // Planned duration
    public int ActualMinutes { get; set; } // Actual time spent
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public Guid UserId { get; set; }
    
    // Navigation Properties
    public User User { get; set; } = default!;
    public TaskItem? Task { get; set; }
}