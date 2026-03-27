namespace TaskManager.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Color { get; set; } = "#48bb78";
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign Key
    public Guid UserId { get; set; }
    
    // Navigation Properties
    public User User { get; set; } = default!;
    // Simplified - no navigation to Tasks for now
}