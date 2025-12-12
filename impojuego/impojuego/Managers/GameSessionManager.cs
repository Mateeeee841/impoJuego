using System.Collections.Concurrent;
using ImpoJuego.Config;
using ImpoJuego.Models;

namespace ImpoJuego.Managers;

/// <summary>
/// Gestiona múltiples sesiones de juego concurrentes
/// </summary>
public class GameSessionManager : IDisposable
{
    private readonly ConcurrentDictionary<string, GameSession> _sessions = new();
    private readonly GameSettings _defaultSettings;
    private readonly Timer _cleanupTimer;
    private readonly TimeSpan _sessionTimeout;

    public GameSessionManager(GameSettings settings, TimeSpan? sessionTimeout = null)
    {
        _defaultSettings = settings;
        _sessionTimeout = sessionTimeout ?? TimeSpan.FromHours(4);

        // Limpiar sesiones expiradas cada 30 minutos
        _cleanupTimer = new Timer(CleanupExpiredSessions, null,
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
    }

    /// <summary>
    /// Obtiene o crea una sesión por su ID
    /// </summary>
    public GameSession GetOrCreateSession(string sessionId)
    {
        return _sessions.GetOrAdd(sessionId, id =>
            new GameSession(id, CloneSettings(_defaultSettings)));
    }

    /// <summary>
    /// Obtiene una sesión existente (null si no existe)
    /// </summary>
    public GameSession? GetSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.Touch();
            return session;
        }
        return null;
    }

    /// <summary>
    /// Elimina una sesión
    /// </summary>
    public bool RemoveSession(string sessionId)
    {
        return _sessions.TryRemove(sessionId, out _);
    }

    /// <summary>
    /// Obtiene el número de sesiones activas
    /// </summary>
    public int ActiveSessionCount => _sessions.Count;

    internal void CleanupExpiredSessions(object? state)
    {
        var cutoff = DateTime.UtcNow - _sessionTimeout;
        var expiredKeys = _sessions
            .Where(kv => kv.Value.LastAccessedAt < cutoff)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _sessions.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            Console.WriteLine($"[GameSessionManager] Limpiadas {expiredKeys.Count} sesiones expiradas. Activas: {_sessions.Count}");
        }
    }

    private static GameSettings CloneSettings(GameSettings original)
    {
        return new GameSettings
        {
            MinPlayers = original.MinPlayers,
            MaxPlayers = original.MaxPlayers,
            TwoImpostorsProbability = original.TwoImpostorsProbability,
            MinPlayersForTwoImpostors = original.MinPlayersForTwoImpostors,
            ImpostorsKnowEachOther = original.ImpostorsKnowEachOther
        };
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}
