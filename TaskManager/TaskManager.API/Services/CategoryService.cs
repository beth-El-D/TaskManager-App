using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Data;
using TaskManager.Domain.Entities;
using TaskManager.API.DTOs;

namespace TaskManager.API.Services;

public class CategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponseDto>> GetUserCategoriesAsync(Guid userId)
    {
        var categories = await _context.Categories
            .Where(c => c.UserId == userId)
            .Include(c => c.Tasks)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToResponseDto).ToList();
    }

    public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid userId, Guid categoryId)
    {
        var category = await _context.Categories
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

        return category != null ? MapToResponseDto(category) : null;
    }

    public async Task<CategoryResponseDto> CreateCategoryAsync(Guid userId, CreateCategoryDto dto)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Color = dto.Color,
            Icon = dto.Icon,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return MapToResponseDto(category);
    }

    public async Task<CategoryResponseDto?> UpdateCategoryAsync(Guid userId, Guid categoryId, UpdateCategoryDto dto)
    {
        var category = await _context.Categories
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

        if (category == null) return null;

        if (!string.IsNullOrEmpty(dto.Name))
            category.Name = dto.Name;

        if (dto.Color != null)
            category.Color = dto.Color;

        if (dto.Icon != null)
            category.Icon = dto.Icon;

        await _context.SaveChangesAsync();

        return MapToResponseDto(category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid userId, Guid categoryId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

        if (category == null) return false;

        // Remove category reference from tasks
        var tasks = await _context.Tasks
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync();

        foreach (var task in tasks)
        {
            task.CategoryId = null;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    private CategoryResponseDto MapToResponseDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Color = category.Color,
            Icon = category.Icon,
            CreatedAt = category.CreatedAt,
            UserId = category.UserId,
            TaskCount = category.Tasks?.Count ?? 0
        };
    }
}