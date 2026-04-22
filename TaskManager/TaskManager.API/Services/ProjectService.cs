using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Data;
using TaskManager.Domain.Entities;
using TaskManager.API.DTOs;

namespace TaskManager.API.Services;

public class ProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectResponseDto>> GetUserProjectsAsync(Guid userId)
    {
        var projects = await _context.Projects
            .Where(p => p.UserId == userId)
            .Include(p => p.Tasks)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return projects.Select(MapToResponseDto).ToList();
    }

    public async Task<ProjectResponseDto?> GetProjectByIdAsync(Guid userId, Guid projectId)
    {
        var project = await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        return project != null ? MapToResponseDto(project) : null;
    }

    public async Task<ProjectResponseDto> CreateProjectAsync(Guid userId, CreateProjectDto dto)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return MapToResponseDto(project);
    }

    public async Task<ProjectResponseDto?> UpdateProjectAsync(Guid userId, Guid projectId, UpdateProjectDto dto)
    {
        var project = await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null) return null;

        if (!string.IsNullOrEmpty(dto.Name))
            project.Name = dto.Name;

        if (dto.Description != null)
            project.Description = dto.Description;

        if (dto.Color != null)
            project.Color = dto.Color;

        if (dto.StartDate.HasValue)
            project.StartDate = dto.StartDate;

        if (dto.EndDate.HasValue)
            project.EndDate = dto.EndDate;

        if (dto.IsCompleted.HasValue)
            project.IsCompleted = dto.IsCompleted.Value;

        await _context.SaveChangesAsync();

        return MapToResponseDto(project);
    }

    public async Task<bool> DeleteProjectAsync(Guid userId, Guid projectId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null) return false;

        // Remove project reference from tasks
        var tasks = await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();

        foreach (var task in tasks)
        {
            task.ProjectId = null;
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    private ProjectResponseDto MapToResponseDto(Project project)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Color = project.Color,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            IsCompleted = project.IsCompleted,
            CreatedAt = project.CreatedAt,
            UserId = project.UserId,
            TaskCount = project.Tasks?.Count ?? 0,
            CompletedTaskCount = project.Tasks?.Count(t => t.IsCompleted) ?? 0
        };
    }
}