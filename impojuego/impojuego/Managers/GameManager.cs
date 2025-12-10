using ImpoJuego.Config;
using ImpoJuego.Data;
using ImpoJuego.Models;

namespace ImpoJuego.Managers;

/// <summary>
/// Resultado del juego
/// </summary>
public enum GameResult
{
    InProgress,
    ImpostorsWin,    // Quedan solo impostores o empate en números
    CrewmatesWin     // Todos los impostores eliminados
}

/// <summary>
/// Información de una ronda para mostrar al jugador
/// </summary>
public class PlayerRoundInfo
{
    public required Player Player { get; init; }
    public required GameRole Role { get; init; }
    public required string Category { get; init; }
    public string? SecretWord { get; init; }  // null para impostores
    public IReadOnlyList<Player>? FellowImpostors { get; init; }  // otros impostores (si aplica)
}

/// <summary>
/// Orquestador principal del juego
/// </summary>
public class GameManager
{
    private readonly Random _random = new();

    public PlayerManager Players { get; }
    public VotingManager Voting { get; }
    public GameSettings Settings { get; }

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Lobby;
    public string CurrentCategory { get; private set; } = string.Empty;
    public string CurrentWord { get; private set; } = string.Empty;
    public int RoundNumber { get; private set; } = 0;

    public GameManager(GameSettings? settings = null)
    {
        Settings = settings ?? new GameSettings();
        Players = new PlayerManager();
        Voting = new VotingManager(Players);
    }

    /// <summary>
    /// Registra un jugador en el lobby
    /// </summary>
    public (bool Success, string Message) RegisterPlayer(string name)
    {
        if (CurrentPhase != GamePhase.Lobby)
            return (false, "Solo se pueden registrar jugadores en el lobby");

        if (Players.Count >= Settings.MaxPlayers)
            return (false, $"Máximo de jugadores alcanzado ({Settings.MaxPlayers})");

        return Players.RegisterPlayer(name);
    }

    /// <summary>
    /// Inicia una nueva partida
    /// </summary>
    public (bool Success, string Message) StartGame()
    {
        if (CurrentPhase != GamePhase.Lobby)
            return (false, "El juego ya está en curso");

        if (Players.Count < Settings.MinPlayers)
            return (false, $"Se necesitan al menos {Settings.MinPlayers} jugadores");

        // Seleccionar categoría y palabra
        CurrentCategory = WordCategories.GetRandomCategory(_random);
        CurrentWord = WordCategories.GetRandomWord(CurrentCategory, _random);

        // Asignar roles
        int impostorCount = Settings.GetImpostorCount(_random, Players.Count);
        Players.AssignRoles(impostorCount, _random);

        CurrentPhase = GamePhase.RoleReveal;
        RoundNumber = 1;

        return (true, $"Partida iniciada - Categoría: {CurrentCategory}");
    }

    /// <summary>
    /// Obtiene la información que debe ver un jugador al revelar su rol
    /// </summary>
    public PlayerRoundInfo? GetPlayerInfo(string playerName)
    {
        var player = Players.GetPlayer(playerName);
        if (player == null || player.IsEliminated)
            return null;

        var info = new PlayerRoundInfo
        {
            Player = player,
            Role = player.Role,
            Category = CurrentCategory,
            SecretWord = player.Role == GameRole.Crewmate ? CurrentWord : null,
            FellowImpostors = player.Role == GameRole.Impostor && Settings.ImpostorsKnowEachOther
                ? Players.GetActiveImpostors().Where(p => p != player).ToList()
                : null
        };

        return info;
    }

    /// <summary>
    /// Avanza a la fase de discusión
    /// </summary>
    public void StartDiscussion()
    {
        CurrentPhase = GamePhase.Discussion;
    }

    /// <summary>
    /// Inicia la fase de votación
    /// </summary>
    public void StartVoting()
    {
        CurrentPhase = GamePhase.Voting;
        Voting.ResetVotes();
    }

    /// <summary>
    /// Procesa el resultado de la votación y verifica condiciones de victoria
    /// </summary>
    public (VoteResult VoteResult, GameResult GameStatus, string Message) ProcessVotingResult()
    {
        var voteResult = Voting.TallyVotes();
        string message;

        if (voteResult.WasTie)
        {
            message = "Empate en la votación. Nadie es eliminado.";
        }
        else if (voteResult.EliminatedPlayer == null)
        {
            message = "La mayoría votó skip. Nadie es eliminado.";
        }
        else
        {
            var eliminated = voteResult.EliminatedPlayer;
            var wasImpostor = eliminated.Role == GameRole.Impostor;
            message = $"{eliminated.Name} fue eliminado. " +
                      (wasImpostor ? "¡Era IMPOSTOR!" : "Era inocente...");
        }

        var gameStatus = CheckWinCondition();

        if (gameStatus == GameResult.InProgress)
        {
            RoundNumber++;
            CurrentPhase = GamePhase.Discussion;
        }
        else
        {
            CurrentPhase = GamePhase.Finished;
        }

        return (voteResult, gameStatus, message);
    }

    /// <summary>
    /// Verifica las condiciones de victoria
    /// </summary>
    public GameResult CheckWinCondition()
    {
        var activeImpostors = Players.GetActiveImpostors().Count;
        var activeCrewmates = Players.GetActiveCrewmates().Count;

        // Crewmates ganan si no quedan impostores
        if (activeImpostors == 0)
            return GameResult.CrewmatesWin;

        // Impostores ganan si igualan o superan a los crewmates
        if (activeImpostors >= activeCrewmates)
            return GameResult.ImpostorsWin;

        return GameResult.InProgress;
    }

    /// <summary>
    /// Obtiene el mensaje de victoria
    /// </summary>
    public string GetWinMessage(GameResult result)
    {
        // Usar GetAllPlayersByRole para incluir impostores eliminados
        var impostors = Players.GetAllPlayersByRole(GameRole.Impostor);
        var impostorNames = string.Join(", ", impostors.Select(p => p.Name));

        return result switch
        {
            GameResult.CrewmatesWin =>
                $"¡VICTORIA DE LOS CREWMATES!\nLos impostores eran: {impostorNames}\nLa palabra era: {CurrentWord}",
            GameResult.ImpostorsWin =>
                $"¡VICTORIA DE LOS IMPOSTORES!\nLos impostores eran: {impostorNames}\nLa palabra era: {CurrentWord}",
            _ => "El juego continúa..."
        };
    }

    /// <summary>
    /// Reinicia el juego para una nueva partida (mantiene jugadores)
    /// </summary>
    public void ResetGame()
    {
        CurrentPhase = GamePhase.Lobby;
        CurrentCategory = string.Empty;
        CurrentWord = string.Empty;
        RoundNumber = 0;
        Voting.ResetVotes();
        // Los jugadores se mantienen, solo se resetean sus estados
        Players.ResetForNewGame();
    }

    /// <summary>
    /// Reinicia completamente (borra jugadores)
    /// </summary>
    public void FullReset()
    {
        ResetGame();
        Players.Clear();
    }

    /// <summary>
    /// Obtiene información del estado actual (para debug/UI)
    /// </summary>
    public string GetGameStatus()
    {
        return $"""
            Fase: {CurrentPhase}
            Ronda: {RoundNumber}
            Categoría: {CurrentCategory}
            Jugadores activos: {Players.ActiveCount}/{Players.Count}
            Impostores activos: {Players.GetActiveImpostors().Count}
            """;
    }
}
