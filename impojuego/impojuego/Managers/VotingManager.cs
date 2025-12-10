using ImpoJuego.Models;

namespace ImpoJuego.Managers;

/// <summary>
/// Resultado de una votación
/// </summary>
public class VoteResult
{
    public Player? EliminatedPlayer { get; init; }
    public bool WasTie { get; init; }
    public Dictionary<Player, int> VoteCounts { get; init; } = new();
    public int SkipVotes { get; init; }

    public bool SomeoneWasEliminated => EliminatedPlayer != null;
}

/// <summary>
/// Gestiona las votaciones del juego
/// </summary>
public class VotingManager
{
    private readonly Dictionary<Player, Player?> _votes = new(); // Votante -> Votado (null = skip)
    private readonly PlayerManager _playerManager;

    public VotingManager(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }

    /// <summary>
    /// Reinicia los votos para una nueva ronda
    /// </summary>
    public void ResetVotes()
    {
        _votes.Clear();
    }

    /// <summary>
    /// Registra un voto
    /// </summary>
    public (bool Success, string Message) CastVote(Player voter, Player? target)
    {
        if (voter.IsEliminated)
            return (false, "Los jugadores eliminados no pueden votar");

        if (target != null && target.IsEliminated)
            return (false, "No puedes votar por un jugador eliminado");

        if (target != null && target == voter)
            return (false, "No puedes votar por ti mismo");

        _votes[voter] = target;

        var targetName = target?.Name ?? "Skip";
        return (true, $"{voter.Name} votó por {targetName}");
    }

    /// <summary>
    /// Verifica si un jugador ya votó
    /// </summary>
    public bool HasVoted(Player player) => _votes.ContainsKey(player);

    /// <summary>
    /// Obtiene cuántos jugadores han votado
    /// </summary>
    public int VotesCast => _votes.Count;

    /// <summary>
    /// Verifica si todos los jugadores activos han votado
    /// </summary>
    public bool AllVotesIn()
    {
        var activePlayers = _playerManager.GetActivePlayers();
        return activePlayers.All(p => _votes.ContainsKey(p));
    }

    /// <summary>
    /// Cuenta los votos y determina el resultado
    /// </summary>
    public VoteResult TallyVotes()
    {
        var voteCounts = new Dictionary<Player, int>();
        int skipVotes = 0;

        // Contar votos
        foreach (var vote in _votes.Values)
        {
            if (vote == null)
            {
                skipVotes++;
            }
            else
            {
                voteCounts.TryGetValue(vote, out int count);
                voteCounts[vote] = count + 1;
            }
        }

        // Encontrar el máximo
        int maxVotes = 0;
        if (voteCounts.Count > 0)
            maxVotes = voteCounts.Values.Max();

        // Skip gana si tiene más votos
        if (skipVotes > maxVotes)
        {
            return new VoteResult
            {
                EliminatedPlayer = null,
                WasTie = false,
                VoteCounts = voteCounts,
                SkipVotes = skipVotes
            };
        }

        // Verificar empate
        var topVoted = voteCounts.Where(kv => kv.Value == maxVotes).ToList();

        if (topVoted.Count > 1 || (topVoted.Count == 1 && skipVotes == maxVotes))
        {
            return new VoteResult
            {
                EliminatedPlayer = null,
                WasTie = true,
                VoteCounts = voteCounts,
                SkipVotes = skipVotes
            };
        }

        // Hay un ganador claro
        var eliminated = topVoted.FirstOrDefault().Key;
        eliminated?.Eliminate();

        return new VoteResult
        {
            EliminatedPlayer = eliminated,
            WasTie = false,
            VoteCounts = voteCounts,
            SkipVotes = skipVotes
        };
    }

    /// <summary>
    /// Obtiene un resumen de los votos actuales (para mostrar)
    /// </summary>
    public string GetVoteSummary()
    {
        var lines = new List<string>();

        foreach (var (voter, target) in _votes)
        {
            var targetName = target?.Name ?? "Skip";
            lines.Add($"  {voter.Name} → {targetName}");
        }

        return string.Join("\n", lines);
    }
}
