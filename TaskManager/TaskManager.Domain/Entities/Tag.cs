namespace TaskManager.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Color { get; set; } = "#e53e3e";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign Key
    public Guid UserId { get; set; }
    
    // Navigation Properties
    public User User { get; set; } = default!;
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}

// Junction table for many-to-many relationship between TaskItem and Tag
public class TaskTag
{
    public Guid TaskId { get; set; }
    public Guid TagId { get; set; }
    
    // Navigation Properties
    public TaskItem Task { get; set; } = default!;
    public Tag Tag { get; set; } = default!;
}