namespace TaskManager.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Color { get; set; } = "#667eea";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign Key
    public Guid UserId { get; set; }
    
    // Navigation Properties
    public User User { get; set; } = default!;
    // Simplified - no navigation to Tasks for now
}