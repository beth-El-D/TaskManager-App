using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManager.API.DTOs;
using TaskManager.API.Services;
using System.Security.Claims;

[ApiController]
[Route("api/focus-sessions")]
[Authorize]
public class FocusSessionController : ControllerBase
{
    private readonly FocusSessionService _focusSessionService;

    public FocusSessionController(FocusSessionService focusSessionService)
    {
        _focusSessionService = focusSessionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFocusSessions([FromQuery] int? limit = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var sessions = await _focusSessionService.GetUserFocusSessionsAsync(userId, limit);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSession()
    {
        try
        {
            var userId = GetCurrentUserId();
            var session = await _focusSessionService.GetActiveFocusSessionAsync(userId);
            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetFocusStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var stats = await _focusSessionService.GetFocusStatsAsync(userId, startDate, endDate);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartFocusSession(CreateFocusSessionDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var session = await _focusSessionService.StartFocusSessionAsync(userId, dto);
            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/end")]
    public async Task<IActionResult> EndFocusSession(Guid id, UpdateFocusSessionDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var session = await _focusSessionService.EndFocusSessionAsync(userId, id, dto);
            
            if (session == null)
                return NotFound("Focus session not found");

            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFocusSession(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _focusSessionService.DeleteFocusSessionAsync(userId, id);
            
            if (!success)
                return NotFound("Focus session not found");

            return Ok("Focus session deleted successfully");
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