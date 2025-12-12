using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ImpoJuego.Api.DTOs;
using ImpoJuego.Api.Services;

namespace ImpoJuego.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest request)
    {
        var (user, error) = await _authService.RegisterAsync(request.Email, request.Password);

        if (error != null)
            return BadRequest(new ApiResponse<object>(false, error, null));

        var token = _authService.GenerateJwtToken(user!);

        return Ok(new ApiResponse<AuthResponseDto>(
            true,
            "Usuario registrado exitosamente",
            new AuthResponseDto(token, new UserDto(user!.Id, user.Email, user.Role))
        ));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        var (user, error) = await _authService.LoginAsync(request.Email, request.Password);

        if (error != null)
            return BadRequest(new ApiResponse<object>(false, error, null));

        var token = _authService.GenerateJwtToken(user!);

        return Ok(new ApiResponse<AuthResponseDto>(
            true,
            "Login exitoso",
            new AuthResponseDto(token, new UserDto(user!.Id, user.Email, user.Role))
        ));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new ApiResponse<object>(false, "Token inválido", null));

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(new ApiResponse<object>(false, "Usuario no encontrado", null));

        return Ok(new ApiResponse<UserDto>(
            true,
            "Usuario encontrado",
            new UserDto(user.Id, user.Email, user.Role)
        ));
    }
}
