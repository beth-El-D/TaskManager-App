namespace TaskManager.Domain.Entities;

public class UserGoal
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public GoalType Type { get; set; } // Daily, Weekly, Monthly
    public int TargetValue { get; set; } // Number of tasks to complete
    public int CurrentValue { get; set; } // Current progress
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }
    
    // Navigation Properties
    public User User { get; set; } = default!;
}

public enum GoalType
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}