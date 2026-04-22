using TaskManager.Domain.Entities;

namespace TaskManager.API.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime DueDate { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? CategoryId { get; set; }
}

public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Priority? Priority { get; set; }
    public Domain.Entities.TaskStatus? Status { get; set; }
    public DateTime? DueDate { get; set; }
    public bool? IsCompleted { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? CategoryId { get; set; }
}

public class TaskResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public Priority Priority { get; set; }
    public Domain.Entities.TaskStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid UserId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? ProjectName { get; set; }
    public string? CategoryName { get; set; }
    public string? ProjectColor { get; set; }
    public string? CategoryColor { get; set; }
}

public class TagResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Color { get; set; }
}