using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Data;
using TaskManager.Domain.Entities;
using TaskManager.API.DTOs;

namespace TaskManager.API.Services;

public class UserPreferenceService
{
    private readonly AppDbContext _context;

    public UserPreferenceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserPreferenceResponseDto>> GetUserPreferencesAsync(Guid userId)
    {
        var preferences = await _context.UserPreferences
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Key)
            .ToListAsync();

        return preferences.Select(MapToResponseDto).ToList();
    }

    public async Task<UserPreferenceResponseDto?> GetPreferenceAsync(Guid userId, string key)
    {
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Key == key);

        return preference != null ? MapToResponseDto(preference) : null;
    }

    public async Task<UserPreferenceResponseDto> SetPreferenceAsync(Guid userId, string key, string value)
    {
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Key == key);

        if (preference != null)
        {
            preference.Value = value;
            preference.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            preference = new UserPreference
            {
                Id = Guid.NewGuid(),
                Key = key,
                Value = value,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.UserPreferences.Add(preference);
        }

        await _context.SaveChangesAsync();
        return MapToResponseDto(preference);
    }

    public async Task<bool> DeletePreferenceAsync(Guid userId, string key)
    {
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Key == key);

        if (preference == null) return false;

        _context.UserPreferences.Remove(preference);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<string, string>> GetPreferencesAsDictionaryAsync(Guid userId)
    {
        var preferences = await _context.UserPreferences
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return preferences.ToDictionary(p => p.Key, p => p.Value);
    }

    public async Task SetDefaultPreferencesAsync(Guid userId)
    {
        var defaultPreferences = new Dictionary<string, string>
        {
            { "default_focus_duration", "25" },
            { "theme", "light" },
            { "dashboard_layout", "default" },
            { "notifications_enabled", "true" },
            { "auto_start_focus", "false" },
            { "break_duration", "5" },
            { "long_break_duration", "15" },
            { "pomodoro_cycles", "4" }
        };

        foreach (var kvp in defaultPreferences)
        {
            var exists = await _context.UserPreferences
                .AnyAsync(p => p.UserId == userId && p.Key == kvp.Key);

            if (!exists)
            {
                await SetPreferenceAsync(userId, kvp.Key, kvp.Value);
            }
        }
    }

    private UserPreferenceResponseDto MapToResponseDto(UserPreference preference)
    {
        return new UserPreferenceResponseDto
        {
            Id = preference.Id,
            Key = preference.Key,
            Value = preference.Value,
            CreatedAt = preference.CreatedAt,
            UpdatedAt = preference.UpdatedAt,
            UserId = preference.UserId
        };
    }
}