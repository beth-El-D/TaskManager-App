namespace TaskManager.API.DTOs;

public class CreateUserPreferenceDto
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
}

public class UpdateUserPreferenceDto
{
    public string Value { get; set; } = default!;
}

public class UserPreferenceResponseDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UserId { get; set; }
}