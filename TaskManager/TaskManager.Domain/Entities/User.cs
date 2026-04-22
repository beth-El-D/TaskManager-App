namespace TaskManager.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<FocusSession> FocusSessions { get; set; } = new List<FocusSession>();
    public ICollection<UserGoal> Goals { get; set; } = new List<UserGoal>();
    public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
}
