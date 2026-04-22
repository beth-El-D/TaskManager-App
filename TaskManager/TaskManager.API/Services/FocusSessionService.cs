using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Data;
using TaskManager.Domain.Entities;
using TaskManager.API.DTOs;

namespace TaskManager.API.Services;

public class FocusSessionService
{
    private readonly AppDbContext _context;

    public FocusSessionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FocusSessionResponseDto>> GetUserFocusSessionsAsync(Guid userId, int? limit = null)
    {
        var query = _context.FocusSessions
            .Where(fs => fs.UserId == userId)
            .Include(fs => fs.Task)
            .OrderByDescending(fs => fs.StartTime);

        if (limit.HasValue)
            query = (IOrderedQueryable<FocusSession>)query.Take(limit.Value);

        var sessions = await query.ToListAsync();

        return sessions.Select(MapToResponseDto).ToList();
    }

    public async Task<FocusSessionResponseDto?> GetActiveFocusSessionAsync(Guid userId)
    {
        var session = await _context.FocusSessions
            .Include(fs => fs.Task)
            .FirstOrDefaultAsync(fs => fs.UserId == userId && !fs.IsCompleted);

        return session != null ? MapToResponseDto(session) : null;
    }

    public async Task<FocusSessionResponseDto> StartFocusSessionAsync(Guid userId, CreateFocusSessionDto dto)
    {
        // End any existing active session
        var activeSession = await _context.FocusSessions
            .FirstOrDefaultAsync(fs => fs.UserId == userId && !fs.IsCompleted);

        if (activeSession != null)
        {
            activeSession.IsCompleted = true;
            activeSession.EndTime = DateTime.UtcNow;
            activeSession.ActualMinutes = (int)(DateTime.UtcNow - activeSession.StartTime).TotalMinutes;
        }

        var session = new FocusSession
        {
            Id = Guid.NewGuid(),
            TaskTitle = dto.TaskTitle,
            TaskId = dto.TaskId,
            DurationMinutes = dto.DurationMinutes,
            StartTime = DateTime.UtcNow,
            Notes = dto.Notes,
            UserId = userId
        };

        _context.FocusSessions.Add(session);
        await _context.SaveChangesAsync();

        return MapToResponseDto(session);
    }

    public async Task<FocusSessionResponseDto?> EndFocusSessionAsync(Guid userId, Guid sessionId, UpdateFocusSessionDto dto)
    {
        var session = await _context.FocusSessions
            .Include(fs => fs.Task)
            .FirstOrDefaultAsync(fs => fs.Id == sessionId && fs.UserId == userId);

        if (session == null) return null;

        session.EndTime = dto.EndTime ?? DateTime.UtcNow;
        session.IsCompleted = dto.IsCompleted ?? true;
        session.ActualMinutes = dto.ActualMinutes ?? (int)(session.EndTime.Value - session.StartTime).TotalMinutes;

        if (dto.Notes != null)
            session.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return MapToResponseDto(session);
    }

    public async Task<bool> DeleteFocusSessionAsync(Guid userId, Guid sessionId)
    {
        var session = await _context.FocusSessions
            .FirstOrDefaultAsync(fs => fs.Id == sessionId && fs.UserId == userId);

        if (session == null) return false;

        _context.FocusSessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<FocusSessionStatsDto> GetFocusStatsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.Today.AddDays(-30); // Last 30 days by default
        endDate ??= DateTime.Today.AddDays(1);

        var sessions = await _context.FocusSessions
            .Where(fs => fs.UserId == userId && fs.StartTime >= startDate && fs.StartTime < endDate)
            .ToListAsync();

        var completedSessions = sessions.Where(s => s.IsCompleted).ToList();

        return new FocusSessionStatsDto
        {
            TotalSessions = sessions.Count,
            CompletedSessions = completedSessions.Count,
            TotalMinutes = completedSessions.Sum(s => s.ActualMinutes),
            AverageSessionLength = completedSessions.Any() ? completedSessions.Average(s => s.ActualMinutes) : 0,
            LongestSession = completedSessions.Any() ? completedSessions.Max(s => s.ActualMinutes) : 0
        };
    }

    private FocusSessionResponseDto MapToResponseDto(FocusSession session)
    {
        return new FocusSessionResponseDto
        {
            Id = session.Id,
            TaskTitle = session.TaskTitle,
            TaskId = session.TaskId,
            DurationMinutes = session.DurationMinutes,
            ActualMinutes = session.ActualMinutes,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            IsCompleted = session.IsCompleted,
            Notes = session.Notes,
            UserId = session.UserId,
            Task = session.Task != null ? new TaskResponseDto
            {
                Id = session.Task.Id,
                Title = session.Task.Title,
                Priority = session.Task.Priority,
                Status = session.Task.Status,
                DueDate = session.Task.DueDate,
                IsCompleted = session.Task.IsCompleted,
                CreatedAt = session.Task.CreatedAt,
                CompletedAt = session.Task.CompletedAt,
                UserId = session.Task.UserId
            } : null
        };
    }
}

public class FocusSessionStatsDto
{
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int TotalMinutes { get; set; }
    public double AverageSessionLength { get; set; }
    public int LongestSession { get; set; }
}