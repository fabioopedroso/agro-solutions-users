using Application.Contracts;
using Application.DTOs.Auth.Signature;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Users.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var token = await _authService.GenerateToken(loginDto);
        return Ok(new { token });
    }
}
