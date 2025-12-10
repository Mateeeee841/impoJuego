namespace ImpoJuego.Api.DTOs;

// === REQUEST DTOs ===

public record RegisterPlayerRequest(string Name);

public record StartGameRequest(int? ImpostorCount = null);

public record RevealRoleRequest(string PlayerName);

public record CastVoteRequest(string VoterName, string? TargetName); // null = skip

// === RESPONSE DTOs ===

public record ApiResponse<T>(bool Success, string Message, T? Data = default);

public record PlayerDto(string Name, bool IsEliminated);

public record LobbyStatusDto(
    List<PlayerDto> Players,
    int MinPlayers,
    int MaxPlayers,
    bool CanStart
);

public record GameStartedDto(
    string Category,
    int ImpostorCount,
    List<string> Players
);

public record PlayerRoleDto(
    string PlayerName,
    string Role,           // "Impostor" o "Crewmate"
    string Category,
    string? SecretWord,    // null para impostores
    List<string>? FellowImpostors  // otros impostores (si aplica)
);

public record GameStateDto(
    string Phase,          // Lobby, RoleReveal, Discussion, Voting, Finished
    int RoundNumber,
    string Category,
    List<PlayerDto> ActivePlayers,
    int ImpostorCount
);

public record VoteResultDto(
    string? EliminatedPlayer,
    bool WasTie,
    Dictionary<string, int> VoteCounts,
    int SkipVotes
);

public record RoundResultDto(
    VoteResultDto VoteResult,
    string GameStatus,     // InProgress, ImpostorsWin, CrewmatesWin
    string Message
);

public record GameEndDto(
    string Winner,         // "Crewmates" o "Impostores"
    List<string> Impostors,
    string SecretWord
);

public record CategoryDto(string Name, int WordCount);
