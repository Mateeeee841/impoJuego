using ImpoJuego.Models;

namespace ImpoJuego.Managers;

/// <summary>
/// Gestiona el registro y administración de jugadores
/// </summary>
public class PlayerManager
{
    private readonly List<Player> _players = new();
    private readonly Dictionary<string, Player> _playerLookup = new();

    public IReadOnlyList<Player> Players => _players;
    public int Count => _players.Count;
    public int ActiveCount => _players.Count(p => !p.IsEliminated);

    /// <summary>
    /// Registra un nuevo jugador
    /// </summary>
    public (bool Success, string Message) RegisterPlayer(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "El nombre no puede estar vacío");

        var normalizedName = name.ToLower().Trim();

        if (_playerLookup.ContainsKey(normalizedName))
            return (false, $"Ya existe un jugador llamado '{name}'");

        var player = new Player(name);
        _players.Add(player);
        _playerLookup[normalizedName] = player;

        return (true, $"Jugador '{player.Name}' registrado");
    }

    /// <summary>
    /// Elimina un jugador del registro
    /// </summary>
    public bool RemovePlayer(string name)
    {
        var normalizedName = name.ToLower().Trim();

        if (!_playerLookup.TryGetValue(normalizedName, out var player))
            return false;

        _players.Remove(player);
        _playerLookup.Remove(normalizedName);
        return true;
    }

    /// <summary>
    /// Busca un jugador por nombre
    /// </summary>
    public Player? GetPlayer(string name)
    {
        var normalizedName = name.ToLower().Trim();
        return _playerLookup.TryGetValue(normalizedName, out var player) ? player : null;
    }

    /// <summary>
    /// Verifica si un jugador existe
    /// </summary>
    public bool PlayerExists(string name)
        => _playerLookup.ContainsKey(name.ToLower().Trim());

    /// <summary>
    /// Obtiene jugadores activos (no eliminados)
    /// </summary>
    public IReadOnlyList<Player> GetActivePlayers()
        => _players.Where(p => !p.IsEliminated).ToList();

    /// <summary>
    /// Obtiene jugadores por rol (solo activos)
    /// </summary>
    public IReadOnlyList<Player> GetPlayersByRole(GameRole role)
        => _players.Where(p => p.Role == role && !p.IsEliminated).ToList();

    /// <summary>
    /// Obtiene TODOS los jugadores por rol (incluye eliminados) - para mostrar al final
    /// </summary>
    public IReadOnlyList<Player> GetAllPlayersByRole(GameRole role)
        => _players.Where(p => p.Role == role).ToList();

    /// <summary>
    /// Obtiene los impostores activos
    /// </summary>
    public IReadOnlyList<Player> GetActiveImpostors()
        => GetPlayersByRole(GameRole.Impostor);

    /// <summary>
    /// Obtiene los crewmates activos
    /// </summary>
    public IReadOnlyList<Player> GetActiveCrewmates()
        => GetPlayersByRole(GameRole.Crewmate);

    /// <summary>
    /// Asigna roles a los jugadores
    /// </summary>
    public void AssignRoles(int impostorCount, Random random)
    {
        // Reset todos a Crewmate
        foreach (var player in _players)
            player.AssignRole(GameRole.Crewmate);

        // Seleccionar impostores al azar
        var shuffled = _players.OrderBy(_ => random.Next()).ToList();
        var impostorsToAssign = Math.Min(impostorCount, _players.Count - 1);

        for (int i = 0; i < impostorsToAssign; i++)
            shuffled[i].AssignRole(GameRole.Impostor);
    }

    /// <summary>
    /// Reinicia el estado de todos los jugadores para nueva partida
    /// </summary>
    public void ResetForNewGame()
    {
        foreach (var player in _players)
        {
            player.Reset();
        }
    }

    /// <summary>
    /// Limpia todos los jugadores
    /// </summary>
    public void Clear()
    {
        _players.Clear();
        _playerLookup.Clear();
    }
}
