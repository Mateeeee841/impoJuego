namespace ImpoJuego.Managers;

/// <summary>
/// Opciones disponibles en el menú
/// </summary>
public enum MenuOption
{
    ResetGame,          // Reiniciar partida (mantiene jugadores)
    FullReset,          // Reiniciar todo (borra jugadores)
    BackToLobby         // Volver al lobby
}

/// <summary>
/// Resultado de una acción del menú
/// </summary>
public class MenuActionResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public MenuOption? ExecutedAction { get; init; }
}

/// <summary>
/// Gestiona las opciones del menú del juego
/// </summary>
public class MenuManager
{
    private readonly GameManager _gameManager;

    public MenuManager(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    /// <summary>
    /// Ejecuta una opción del menú
    /// </summary>
    public MenuActionResult ExecuteOption(MenuOption option)
    {
        return option switch
        {
            MenuOption.ResetGame => ResetGame(),
            MenuOption.FullReset => FullReset(),
            MenuOption.BackToLobby => BackToLobby(),
            _ => new MenuActionResult
            {
                Success = false,
                Message = "Opción no válida"
            }
        };
    }

    /// <summary>
    /// Reinicia la partida manteniendo los jugadores
    /// </summary>
    public MenuActionResult ResetGame()
    {
        _gameManager.ResetGame();
        return new MenuActionResult
        {
            Success = true,
            Message = "Partida reiniciada. Los jugadores se mantienen.",
            ExecutedAction = MenuOption.ResetGame
        };
    }

    /// <summary>
    /// Reinicia completamente el juego (borra jugadores)
    /// </summary>
    public MenuActionResult FullReset()
    {
        _gameManager.FullReset();
        return new MenuActionResult
        {
            Success = true,
            Message = "Juego reiniciado completamente.",
            ExecutedAction = MenuOption.FullReset
        };
    }

    /// <summary>
    /// Vuelve al lobby (equivalente a ResetGame)
    /// </summary>
    public MenuActionResult BackToLobby()
    {
        _gameManager.ResetGame();
        return new MenuActionResult
        {
            Success = true,
            Message = "Volviendo al lobby.",
            ExecutedAction = MenuOption.BackToLobby
        };
    }

    /// <summary>
    /// Obtiene las opciones disponibles según el estado actual
    /// </summary>
    public IReadOnlyList<MenuOption> GetAvailableOptions()
    {
        var options = new List<MenuOption>();

        // Siempre disponible si hay partida en curso o terminada
        if (_gameManager.CurrentPhase != Models.GamePhase.Lobby)
        {
            options.Add(MenuOption.ResetGame);
            options.Add(MenuOption.BackToLobby);
        }

        // Full reset siempre disponible si hay jugadores
        if (_gameManager.Players.Count > 0)
        {
            options.Add(MenuOption.FullReset);
        }

        return options;
    }
}
