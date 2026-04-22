using TaskManager.Domain.Entities;

namespace TaskManager.API.DTOs;

public class CreateUserGoalDto
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public GoalType Type { get; set; }
    public int TargetValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class UpdateUserGoalDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? TargetValue { get; set; }
    public int? CurrentValue { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}

public class UserGoalResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public GoalType Type { get; set; }
    public int TargetValue { get; set; }
    public int CurrentValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public double ProgressPercentage => TargetValue > 0 ? (double)CurrentValue / TargetValue * 100 : 0;
}