using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Data;
using TaskManager.Domain.Entities;
using TaskManager.API.DTOs;

namespace TaskManager.API.Services;

public class UserGoalService
{
    private readonly AppDbContext _context;

    public UserGoalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserGoalResponseDto>> GetUserGoalsAsync(Guid userId, bool activeOnly = false)
    {
        var query = _context.UserGoals
            .Where(g => g.UserId == userId);

        if (activeOnly)
            query = query.Where(g => g.IsActive);

        var goals = await query
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return goals.Select(MapToResponseDto).ToList();
    }

    public async Task<UserGoalResponseDto?> GetGoalByIdAsync(Guid userId, Guid goalId)
    {
        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId);

        return goal != null ? MapToResponseDto(goal) : null;
    }

    public async Task<UserGoalResponseDto> CreateGoalAsync(Guid userId, CreateUserGoalDto dto)
    {
        var goal = new UserGoal
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            TargetValue = dto.TargetValue,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserGoals.Add(goal);
        await _context.SaveChangesAsync();

        return MapToResponseDto(goal);
    }

    public async Task<UserGoalResponseDto?> UpdateGoalAsync(Guid userId, Guid goalId, UpdateUserGoalDto dto)
    {
        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId);

        if (goal == null) return null;

        if (!string.IsNullOrEmpty(dto.Title))
            goal.Title = dto.Title;

        if (dto.Description != null)
            goal.Description = dto.Description;

        if (dto.TargetValue.HasValue)
            goal.TargetValue = dto.TargetValue.Value;

        if (dto.CurrentValue.HasValue)
            goal.CurrentValue = dto.CurrentValue.Value;

        if (dto.StartDate.HasValue)
            goal.StartDate = dto.StartDate.Value;

        if (dto.EndDate.HasValue)
            goal.EndDate = dto.EndDate.Value;

        if (dto.IsActive.HasValue)
            goal.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return MapToResponseDto(goal);
    }

    public async Task<bool> DeleteGoalAsync(Guid userId, Guid goalId)
    {
        var goal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId);

        if (goal == null) return false;

        _context.UserGoals.Remove(goal);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateGoalProgressAsync(Guid userId)
    {
        var activeGoals = await _context.UserGoals
            .Where(g => g.UserId == userId && g.IsActive)
            .ToListAsync();

        foreach (var goal in activeGoals)
        {
            var currentValue = await CalculateGoalProgress(userId, goal);
            goal.CurrentValue = currentValue;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<int> CalculateGoalProgress(Guid userId, UserGoal goal)
    {
        return goal.Type switch
        {
            GoalType.Daily => await _context.Tasks
                .Where(t => t.UserId == userId && t.IsCompleted && t.CompletedAt.HasValue && t.CompletedAt.Value.Date == DateTime.Today)
                .CountAsync(),
            
            GoalType.Weekly => await _context.Tasks
                .Where(t => t.UserId == userId && t.IsCompleted && t.CompletedAt.HasValue && 
                           t.CompletedAt.Value >= goal.StartDate && t.CompletedAt.Value <= goal.EndDate)
                .CountAsync(),
            
            GoalType.Monthly => await _context.Tasks
                .Where(t => t.UserId == userId && t.IsCompleted && t.CompletedAt.HasValue && 
                           t.CompletedAt.Value >= goal.StartDate && t.CompletedAt.Value <= goal.EndDate)
                .CountAsync(),
            
            _ => 0
        };
    }

    private UserGoalResponseDto MapToResponseDto(UserGoal goal)
    {
        return new UserGoalResponseDto
        {
            Id = goal.Id,
            Title = goal.Title,
            Description = goal.Description,
            Type = goal.Type,
            TargetValue = goal.TargetValue,
            CurrentValue = goal.CurrentValue,
            StartDate = goal.StartDate,
            EndDate = goal.EndDate,
            IsActive = goal.IsActive,
            CreatedAt = goal.CreatedAt,
            UserId = goal.UserId
        };
    }
}