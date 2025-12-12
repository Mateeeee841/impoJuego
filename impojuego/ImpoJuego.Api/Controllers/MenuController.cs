using Microsoft.AspNetCore.Mvc;
using ImpoJuego.Managers;

namespace ImpoJuego.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : SessionControllerBase
{
    public MenuController(GameSessionManager sessionManager) : base(sessionManager)
    {
    }

    private MenuManager GetMenu() => new MenuManager(GetGame());

    /// <summary>
    /// GET /api/menu/options - Obtener opciones disponibles
    /// </summary>
    [HttpGet("options")]
    public ActionResult<MenuResponse<List<string>>> GetOptions()
    {
        var options = GetMenu().GetAvailableOptions()
            .Select(o => o.ToString())
            .ToList();

        return Ok(new MenuResponse<List<string>>(true, "Opciones disponibles", options));
    }

    /// <summary>
    /// POST /api/menu/reset - Reiniciar partida (mantiene jugadores)
    /// </summary>
    [HttpPost("reset")]
    public ActionResult<MenuResponse<MenuActionResultDto>> ResetGame()
    {
        var result = GetMenu().ResetGame();
        return Ok(ToResponse(result));
    }

    /// <summary>
    /// POST /api/menu/full-reset - Reiniciar todo
    /// </summary>
    [HttpPost("full-reset")]
    public ActionResult<MenuResponse<MenuActionResultDto>> FullReset()
    {
        var result = GetMenu().FullReset();
        return Ok(ToResponse(result));
    }

    /// <summary>
    /// POST /api/menu/back-to-lobby - Volver al lobby
    /// </summary>
    [HttpPost("back-to-lobby")]
    public ActionResult<MenuResponse<MenuActionResultDto>> BackToLobby()
    {
        var result = GetMenu().BackToLobby();
        return Ok(ToResponse(result));
    }

    /// <summary>
    /// POST /api/menu/action - Ejecutar acción por nombre
    /// </summary>
    [HttpPost("action")]
    public ActionResult<MenuResponse<MenuActionResultDto>> ExecuteAction([FromBody] MenuActionRequest request)
    {
        if (!Enum.TryParse<MenuOption>(request.Action, true, out var option))
        {
            return BadRequest(new MenuResponse<MenuActionResultDto>(false, $"Acción '{request.Action}' no válida", null));
        }

        var result = GetMenu().ExecuteOption(option);
        return Ok(ToResponse(result));
    }

    private static MenuResponse<MenuActionResultDto> ToResponse(MenuActionResult result)
    {
        var dto = new MenuActionResultDto(
            result.Success,
            result.Message,
            result.ExecutedAction?.ToString()
        );
        return new MenuResponse<MenuActionResultDto>(result.Success, result.Message, dto);
    }
}

// DTOs para el MenuController
public record MenuResponse<T>(bool Success, string Message, T? Data = default);
public record MenuActionRequest(string Action);
public record MenuActionResultDto(bool Success, string Message, string? ExecutedAction);
