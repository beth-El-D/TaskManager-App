namespace TaskManager.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // Foreign Keys
    public Guid UserId { get; set; }
    public Guid? ProjectId { get; set; } // Optional project assignment
    public Guid? CategoryId { get; set; } // Optional category assignment
    
    // Navigation Properties
    public User User { get; set; } = default!;
    public Project? Project { get; set; }
    public Category? Category { get; set; }
    public ICollection<FocusSession> FocusSessions { get; set; } = new List<FocusSession>();
}

public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3
}

public enum TaskStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}