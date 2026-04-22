using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManager.API.DTOs;
using TaskManager.API.Services;
using System.Security.Claims;

[ApiController]
[Route("api/goals")]
[Authorize]
public class UserGoalController : ControllerBase
{
    private readonly UserGoalService _goalService;

    public UserGoalController(UserGoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetGoals([FromQuery] bool activeOnly = false)
    {
        try
        {
            var userId = GetCurrentUserId();
            var goals = await _goalService.GetUserGoalsAsync(userId, activeOnly);
            return Ok(goals);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGoal(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var goal = await _goalService.GetGoalByIdAsync(userId, id);
            
            if (goal == null)
                return NotFound("Goal not found");

            return Ok(goal);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoal(CreateUserGoalDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var goal = await _goalService.CreateGoalAsync(userId, dto);
            return Ok(goal);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGoal(Guid id, UpdateUserGoalDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var goal = await _goalService.UpdateGoalAsync(userId, id, dto);
            
            if (goal == null)
                return NotFound("Goal not found");

            return Ok(goal);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("update-progress")]
    public async Task<IActionResult> UpdateGoalProgress()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _goalService.UpdateGoalProgressAsync(userId);
            return Ok("Goal progress updated successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGoal(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _goalService.DeleteGoalAsync(userId, id);
            
            if (!success)
                return NotFound("Goal not found");

            return Ok("Goal deleted successfully");
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