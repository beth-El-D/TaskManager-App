using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManager.API.DTOs;
using TaskManager.API.Services;
using System.Security.Claims;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var userId = GetCurrentUserId();
            var categories = await _categoryService.GetUserCategoriesAsync(userId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var category = await _categoryService.GetCategoryByIdAsync(userId, id);
            
            if (category == null)
                return NotFound("Category not found");

            return Ok(category);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var category = await _categoryService.CreateCategoryAsync(userId, dto);
            return Ok(category);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var category = await _categoryService.UpdateCategoryAsync(userId, id, dto);
            
            if (category == null)
                return NotFound("Category not found");

            return Ok(category);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _categoryService.DeleteCategoryAsync(userId, id);
            
            if (!success)
                return NotFound("Category not found");

            return Ok("Category deleted successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        return userId;
    }
}