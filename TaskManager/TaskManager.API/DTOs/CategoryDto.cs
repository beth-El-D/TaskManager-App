namespace TaskManager.API.DTOs;

public class CreateCategoryDto
{
    public string Name { get; set; } = default!;
    public string? Color { get; set; }
    public string? Icon { get; set; }
}

public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}

public class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public int TaskCount { get; set; }
}