using ImpoJuego.Config;
using ImpoJuego.Managers;

namespace ImpoJuego.Models;

/// <summary>
/// Representa una sesión de juego con su GameManager y metadata
/// </summary>
public class GameSession
{
    public string SessionId { get; }
    public GameManager Game { get; }
    public DateTime CreatedAt { get; }
    public DateTime LastAccessedAt { get; private set; }

    public GameSession(string sessionId, GameSettings settings)
    {
        SessionId = sessionId;
        Game = new GameManager(settings);
        CreatedAt = DateTime.UtcNow;
        LastAccessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza el timestamp de último acceso
    /// </summary>
    public void Touch()
    {
        LastAccessedAt = DateTime.UtcNow;
    }
}
