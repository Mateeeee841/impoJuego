using Microsoft.AspNetCore.Mvc;
using ImpoJuego.Managers;

namespace ImpoJuego.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly MenuManager _menu;

    public MenuController(MenuManager menu)
    {
        _menu = menu;
    }

    /// <summary>
    /// GET /api/menu/options - Obtener opciones disponibles
    /// </summary>
    [HttpGet("options")]
    public ActionResult<MenuResponse<List<string>>> GetOptions()
    {
        var options = _menu.GetAvailableOptions()
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
        var result = _menu.ResetGame();
        return Ok(ToResponse(result));
    }

    /// <summary>
    /// POST /api/menu/full-reset - Reiniciar todo
    /// </summary>
    [HttpPost("full-reset")]
    public ActionResult<MenuResponse<MenuActionResultDto>> FullReset()
    {
        var result = _menu.FullReset();
        return Ok(ToResponse(result));
    }

    /// <summary>
    /// POST /api/menu/back-to-lobby - Volver al lobby
    /// </summary>
    [HttpPost("back-to-lobby")]
    public ActionResult<MenuResponse<MenuActionResultDto>> BackToLobby()
    {
        var result = _menu.BackToLobby();
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

        var result = _menu.ExecuteOption(option);
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
