namespace TaskManager.Domain.Entities;

public class UserPreference
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!; // e.g., "default_focus_duration", "theme", "dashboard_layout"
    public string Value { get; set; } = default!; // JSON or simple string value
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }
    
    // Navigation Properties
    public User User { get; set; } = default!;
}