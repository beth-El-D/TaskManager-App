namespace TaskManager.API.DTOs;

public class CreateFocusSessionDto
{
    public string? TaskTitle { get; set; }
    public Guid? TaskId { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
}

public class UpdateFocusSessionDto
{
    public int? ActualMinutes { get; set; }
    public DateTime? EndTime { get; set; }
    public bool? IsCompleted { get; set; }
    public string? Notes { get; set; }
}

public class FocusSessionResponseDto
{
    public Guid Id { get; set; }
    public string? TaskTitle { get; set; }
    public Guid? TaskId { get; set; }
    public int DurationMinutes { get; set; }
    public int ActualMinutes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public Guid UserId { get; set; }
    public TaskResponseDto? Task { get; set; }
}