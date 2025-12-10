using Microsoft.AspNetCore.Mvc;
using ImpoJuego.Api.DTOs;
using ImpoJuego.Managers;
using ImpoJuego.Models;
using ImpoJuego.Data;

namespace ImpoJuego.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameManager _game;

    public GameController(GameManager game)
    {
        _game = game;
    }

    // === LOBBY ENDPOINTS ===

    /// <summary>
    /// GET /api/game/lobby - Estado del lobby
    /// </summary>
    [HttpGet("lobby")]
    public ActionResult<ApiResponse<LobbyStatusDto>> GetLobbyStatus()
    {
        var players = _game.Players.Players
            .Select(p => new PlayerDto(p.Name, p.IsEliminated))
            .ToList();

        var data = new LobbyStatusDto(
            Players: players,
            MinPlayers: _game.Settings.MinPlayers,
            MaxPlayers: _game.Settings.MaxPlayers,
            CanStart: _game.Players.Count >= _game.Settings.MinPlayers
        );

        return Ok(new ApiResponse<LobbyStatusDto>(true, "Lobby status", data));
    }

    /// <summary>
    /// POST /api/game/players - Registrar jugador
    /// </summary>
    [HttpPost("players")]
    public ActionResult<ApiResponse<LobbyStatusDto>> RegisterPlayer([FromBody] RegisterPlayerRequest request)
    {
        var (success, message) = _game.RegisterPlayer(request.Name);

        if (!success)
            return BadRequest(new ApiResponse<LobbyStatusDto>(false, message, null));

        // Devolver estado actualizado del lobby
        var players = _game.Players.Players
            .Select(p => new PlayerDto(p.Name, p.IsEliminated))
            .ToList();

        var data = new LobbyStatusDto(
            Players: players,
            MinPlayers: _game.Settings.MinPlayers,
            MaxPlayers: _game.Settings.MaxPlayers,
            CanStart: _game.Players.Count >= _game.Settings.MinPlayers
        );

        return Ok(new ApiResponse<LobbyStatusDto>(true, message, data));
    }

    /// <summary>
    /// DELETE /api/game/players/{name} - Eliminar jugador del lobby
    /// </summary>
    [HttpDelete("players/{name}")]
    public ActionResult<ApiResponse<object>> RemovePlayer(string name)
    {
        if (_game.CurrentPhase != GamePhase.Lobby)
            return BadRequest(new ApiResponse<object>(false, "Solo se pueden eliminar jugadores en el lobby", null));

        var removed = _game.Players.RemovePlayer(name);

        if (!removed)
            return NotFound(new ApiResponse<object>(false, $"Jugador '{name}' no encontrado", null));

        return Ok(new ApiResponse<object>(true, $"Jugador '{name}' eliminado", null));
    }

    // === GAME FLOW ENDPOINTS ===

    /// <summary>
    /// POST /api/game/start - Iniciar partida (impostores se asignan random)
    /// </summary>
    [HttpPost("start")]
    public ActionResult<ApiResponse<GameStartedDto>> StartGame()
    {
        var (success, message) = _game.StartGame();

        if (!success)
            return BadRequest(new ApiResponse<GameStartedDto>(false, message, null));

        var data = new GameStartedDto(
            Category: _game.CurrentCategory,
            ImpostorCount: _game.Players.GetActiveImpostors().Count,
            Players: _game.Players.Players.Select(p => p.Name).ToList()
        );

        return Ok(new ApiResponse<GameStartedDto>(true, message, data));
    }

    /// <summary>
    /// GET /api/game/state - Estado actual del juego
    /// </summary>
    [HttpGet("state")]
    public ActionResult<ApiResponse<GameStateDto>> GetGameState()
    {
        var activePlayers = _game.Players.GetActivePlayers()
            .Select(p => new PlayerDto(p.Name, p.IsEliminated))
            .ToList();

        var data = new GameStateDto(
            Phase: _game.CurrentPhase.ToString(),
            RoundNumber: _game.RoundNumber,
            Category: _game.CurrentCategory,
            ActivePlayers: activePlayers,
            ImpostorCount: _game.Players.GetActiveImpostors().Count
        );

        return Ok(new ApiResponse<GameStateDto>(true, "Game state", data));
    }

    /// <summary>
    /// POST /api/game/reveal - Revelar rol de un jugador
    /// </summary>
    [HttpPost("reveal")]
    public ActionResult<ApiResponse<PlayerRoleDto>> RevealRole([FromBody] RevealRoleRequest request)
    {
        var info = _game.GetPlayerInfo(request.PlayerName);

        if (info == null)
            return NotFound(new ApiResponse<PlayerRoleDto>(false, "Jugador no encontrado o ya eliminado", null));

        var data = new PlayerRoleDto(
            PlayerName: info.Player.Name,
            Role: info.Role.ToString(),
            Category: info.Category,
            SecretWord: info.SecretWord,
            FellowImpostors: info.FellowImpostors?.Select(p => p.Name).ToList()
        );

        return Ok(new ApiResponse<PlayerRoleDto>(true, "Rol revelado", data));
    }

    /// <summary>
    /// POST /api/game/discussion - Iniciar fase de discusión
    /// </summary>
    [HttpPost("discussion")]
    public ActionResult<ApiResponse<GameStateDto>> StartDiscussion()
    {
        _game.StartDiscussion();
        return GetGameState();
    }

    // === VOTING ENDPOINTS ===

    /// <summary>
    /// POST /api/game/voting - Iniciar fase de votación
    /// </summary>
    [HttpPost("voting")]
    public ActionResult<ApiResponse<GameStateDto>> StartVoting()
    {
        _game.StartVoting();
        return GetGameState();
    }

    /// <summary>
    /// POST /api/game/vote - Emitir voto
    /// </summary>
    [HttpPost("vote")]
    public ActionResult<ApiResponse<object>> CastVote([FromBody] CastVoteRequest request)
    {
        var voter = _game.Players.GetPlayer(request.VoterName);
        if (voter == null)
            return NotFound(new ApiResponse<object>(false, "Votante no encontrado", null));

        Player? target = null;
        if (!string.IsNullOrEmpty(request.TargetName))
        {
            target = _game.Players.GetPlayer(request.TargetName);
            if (target == null)
                return NotFound(new ApiResponse<object>(false, "Objetivo no encontrado", null));
        }

        var (success, message) = _game.Voting.CastVote(voter, target);

        if (!success)
            return BadRequest(new ApiResponse<object>(false, message, null));

        return Ok(new ApiResponse<object>(true, message, new { VotesCast = _game.Voting.VotesCast }));
    }

    /// <summary>
    /// GET /api/game/votes - Ver estado de votación
    /// </summary>
    [HttpGet("votes")]
    public ActionResult<ApiResponse<object>> GetVotingStatus()
    {
        var activePlayers = _game.Players.GetActivePlayers().Count;
        var votesCast = _game.Voting.VotesCast;

        return Ok(new ApiResponse<object>(true, "Voting status", new
        {
            VotesCast = votesCast,
            TotalVoters = activePlayers,
            AllVotesIn = _game.Voting.AllVotesIn()
        }));
    }

    /// <summary>
    /// POST /api/game/tally - Procesar resultado de votación
    /// </summary>
    [HttpPost("tally")]
    public ActionResult<ApiResponse<RoundResultDto>> TallyVotes()
    {
        if (!_game.Voting.AllVotesIn())
            return BadRequest(new ApiResponse<RoundResultDto>(false, "No todos han votado", null));

        var (voteResult, gameStatus, message) = _game.ProcessVotingResult();

        var voteResultDto = new VoteResultDto(
            EliminatedPlayer: voteResult.EliminatedPlayer?.Name,
            WasTie: voteResult.WasTie,
            VoteCounts: voteResult.VoteCounts.ToDictionary(kv => kv.Key.Name, kv => kv.Value),
            SkipVotes: voteResult.SkipVotes
        );

        var data = new RoundResultDto(
            VoteResult: voteResultDto,
            GameStatus: gameStatus.ToString(),
            Message: message
        );

        return Ok(new ApiResponse<RoundResultDto>(true, message, data));
    }

    // === GAME END ENDPOINTS ===

    /// <summary>
    /// GET /api/game/result - Resultado final (solo cuando terminó)
    /// </summary>
    [HttpGet("result")]
    public ActionResult<ApiResponse<GameEndDto>> GetGameResult()
    {
        if (_game.CurrentPhase != GamePhase.Finished)
            return BadRequest(new ApiResponse<GameEndDto>(false, "El juego no ha terminado", null));

        var result = _game.CheckWinCondition();
        var impostors = _game.Players.GetAllPlayersByRole(GameRole.Impostor)
            .Select(p => p.Name)
            .ToList();

        var data = new GameEndDto(
            Winner: result == GameResult.CrewmatesWin ? "Crewmates" : "Impostores",
            Impostors: impostors,
            SecretWord: _game.CurrentWord
        );

        return Ok(new ApiResponse<GameEndDto>(true, _game.GetWinMessage(result), data));
    }

    /// <summary>
    /// POST /api/game/reset - Reiniciar juego (mantiene jugadores)
    /// </summary>
    [HttpPost("reset")]
    public ActionResult<ApiResponse<object>> ResetGame()
    {
        _game.ResetGame();
        return Ok(new ApiResponse<object>(true, "Juego reiniciado", null));
    }

    /// <summary>
    /// POST /api/game/full-reset - Reiniciar completamente
    /// </summary>
    [HttpPost("full-reset")]
    public ActionResult<ApiResponse<object>> FullReset()
    {
        _game.FullReset();
        return Ok(new ApiResponse<object>(true, "Juego reiniciado completamente", null));
    }

    // === CATEGORIES ENDPOINT ===

    /// <summary>
    /// GET /api/game/categories - Listar categorías disponibles
    /// </summary>
    [HttpGet("categories")]
    public ActionResult<ApiResponse<List<CategoryDto>>> GetCategories()
    {
        var categories = WordCategories.GetCategoryNames()
            .Select(c => new CategoryDto(c, WordCategories.GetWords(c).Count))
            .ToList();

        return Ok(new ApiResponse<List<CategoryDto>>(true, "Categorías disponibles", categories));
    }
}
