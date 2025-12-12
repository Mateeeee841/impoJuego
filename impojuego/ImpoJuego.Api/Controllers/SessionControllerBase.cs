using Microsoft.AspNetCore.Mvc;
using ImpoJuego.Managers;
using ImpoJuego.Models;

namespace ImpoJuego.Api.Controllers;

/// <summary>
/// Base controller con lógica de sesión compartida
/// </summary>
public abstract class SessionControllerBase : ControllerBase
{
    protected const string SessionHeaderName = "X-Session-Id";

    protected readonly GameSessionManager _sessionManager;

    protected SessionControllerBase(GameSessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    /// <summary>
    /// Obtiene el sessionId del header o retorna null
    /// </summary>
    protected string? GetSessionId()
    {
        if (Request.Headers.TryGetValue(SessionHeaderName, out var sessionId))
        {
            return sessionId.ToString();
        }
        return null;
    }

    /// <summary>
    /// Obtiene la sesión del request o crea una nueva
    /// </summary>
    protected GameSession GetOrCreateSession()
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId))
        {
            // Generar nuevo ID si no viene en header
            sessionId = Guid.NewGuid().ToString();
            Response.Headers.Append(SessionHeaderName, sessionId);
        }

        var session = _sessionManager.GetOrCreateSession(sessionId);
        session.Touch();
        return session;
    }

    /// <summary>
    /// Obtiene el GameManager de la sesión actual
    /// </summary>
    protected GameManager GetGame()
    {
        return GetOrCreateSession().Game;
    }
}
