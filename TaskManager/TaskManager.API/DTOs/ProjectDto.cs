namespace TaskManager.API.DTOs;

public class CreateProjectDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Color { get; set; } = "#667eea";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateProjectDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsCompleted { get; set; }
}

public class ProjectResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
}