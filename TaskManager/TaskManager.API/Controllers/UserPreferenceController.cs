using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManager.API.DTOs;
using TaskManager.API.Services;
using System.Security.Claims;

[ApiController]
[Route("api/preferences")]
[Authorize]
public class UserPreferenceController : ControllerBase
{
    private readonly UserPreferenceService _preferenceService;

    public UserPreferenceController(UserPreferenceService preferenceService)
    {
        _preferenceService = preferenceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPreferences()
    {
        try
        {
            var userId = GetCurrentUserId();
            var preferences = await _preferenceService.GetUserPreferencesAsync(userId);
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("dictionary")]
    public async Task<IActionResult> GetPreferencesAsDictionary()
    {
        try
        {
            var userId = GetCurrentUserId();
            var preferences = await _preferenceService.GetPreferencesAsDictionaryAsync(userId);
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetPreference(string key)
    {
        try
        {
            var userId = GetCurrentUserId();
            var preference = await _preferenceService.GetPreferenceAsync(userId, key);
            
            if (preference == null)
                return NotFound("Preference not found");

            return Ok(preference);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{key}")]
    public async Task<IActionResult> SetPreference(string key, [FromBody] string value)
    {
        try
        {
            var userId = GetCurrentUserId();
            var preference = await _preferenceService.SetPreferenceAsync(userId, key, value);
            return Ok(preference);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("defaults")]
    public async Task<IActionResult> SetDefaultPreferences()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _preferenceService.SetDefaultPreferencesAsync(userId);
            return Ok("Default preferences set successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> DeletePreference(string key)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _preferenceService.DeletePreferenceAsync(userId, key);
            
            if (!success)
                return NotFound("Preference not found");

            return Ok("Preference deleted successfully");
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