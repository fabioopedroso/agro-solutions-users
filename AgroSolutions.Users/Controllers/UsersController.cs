using Application.DTOs.User.Signature;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Users.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserAppService _userAppService;

    public UsersController(UserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        await _userAppService.Register(registerDto);
        return CreatedAtAction(nameof(Register), new { message = "Usuário registrado com sucesso" });
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        await _userAppService.ChangePassword(changePasswordDto);
        return NoContent();
    }
}
