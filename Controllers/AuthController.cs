using ExpenseTracker.DTOs;
using ExpenseTracker.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Validates a Google ID token and returns a JWT for API access.
    /// </summary>
    [HttpPost("google")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        try
        {
            var result = await _authService.GoogleLoginAsync(dto);
            return Ok(result);
        }
        catch (InvalidJwtException)
        {
            return Unauthorized(new { message = "Invalid or expired Google token." });
        }
    }
}
