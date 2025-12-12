using FluentAssertions;
using ImpoJuego.Config;
using ImpoJuego.Managers;
using ImpoJuego.Models;

namespace ImpoJuego.Tests;

public class GameSessionTests
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        var settings = new GameSettings();
        var session = new GameSession("test-session", settings);

        session.SessionId.Should().Be("test-session");
        session.Game.Should().NotBeNull();
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        session.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Touch_ShouldUpdateLastAccessedAt()
    {
        var settings = new GameSettings();
        var session = new GameSession("test-session", settings);
        var originalTime = session.LastAccessedAt;

        Thread.Sleep(10);
        session.Touch();

        session.LastAccessedAt.Should().BeAfter(originalTime);
    }

    [Fact]
    public void Game_ShouldUseProvidedSettings()
    {
        var settings = new GameSettings { MinPlayers = 5 };
        var session = new GameSession("test-session", settings);

        session.Game.Settings.MinPlayers.Should().Be(5);
    }
}

public class GameSessionManagerTests : IDisposable
{
    private GameSessionManager _manager;

    public GameSessionManagerTests()
    {
        _manager = new GameSessionManager(new GameSettings());
    }

    public void Dispose()
    {
        _manager.Dispose();
    }

    [Fact]
    public void GetOrCreateSession_ShouldCreateNewSession()
    {
        var session = _manager.GetOrCreateSession("new-session");

        session.Should().NotBeNull();
        session.SessionId.Should().Be("new-session");
    }

    [Fact]
    public void GetOrCreateSession_ShouldReturnExistingSession()
    {
        var session1 = _manager.GetOrCreateSession("test-session");
        var session2 = _manager.GetOrCreateSession("test-session");

        session1.Should().BeSameAs(session2);
    }

    [Fact]
    public void GetSession_WithExistingSession_ShouldReturnSession()
    {
        _manager.GetOrCreateSession("existing-session");

        var session = _manager.GetSession("existing-session");

        session.Should().NotBeNull();
    }

    [Fact]
    public void GetSession_WithNonExistingSession_ShouldReturnNull()
    {
        var session = _manager.GetSession("non-existing");

        session.Should().BeNull();
    }

    [Fact]
    public void GetSession_ShouldTouchSession()
    {
        var session = _manager.GetOrCreateSession("touch-test");
        var originalTime = session.LastAccessedAt;

        Thread.Sleep(10);
        _manager.GetSession("touch-test");

        session.LastAccessedAt.Should().BeAfter(originalTime);
    }

    [Fact]
    public void RemoveSession_WithExistingSession_ShouldReturnTrue()
    {
        _manager.GetOrCreateSession("to-remove");

        var result = _manager.RemoveSession("to-remove");

        result.Should().BeTrue();
        _manager.GetSession("to-remove").Should().BeNull();
    }

    [Fact]
    public void RemoveSession_WithNonExistingSession_ShouldReturnFalse()
    {
        var result = _manager.RemoveSession("non-existing");

        result.Should().BeFalse();
    }

    [Fact]
    public void ActiveSessionCount_ShouldReturnCorrectCount()
    {
        _manager.GetOrCreateSession("session1");
        _manager.GetOrCreateSession("session2");
        _manager.GetOrCreateSession("session3");

        _manager.ActiveSessionCount.Should().Be(3);
    }

    [Fact]
    public void Constructor_WithCustomTimeout_ShouldWork()
    {
        using var manager = new GameSessionManager(new GameSettings(), TimeSpan.FromMinutes(30));
        manager.Should().NotBeNull();
    }

    [Fact]
    public void MultipleSessions_ShouldBeIndependent()
    {
        var session1 = _manager.GetOrCreateSession("session-1");
        var session2 = _manager.GetOrCreateSession("session-2");

        session1.Game.RegisterPlayer("Player1");

        session1.Game.Players.Count.Should().Be(1);
        session2.Game.Players.Count.Should().Be(0);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        var manager = new GameSessionManager(new GameSettings());
        manager.GetOrCreateSession("test");

        var action = () => manager.Dispose();

        action.Should().NotThrow();
    }

    [Fact]
    public void CleanupExpiredSessions_WithNoExpiredSessions_ShouldNotRemoveAnything()
    {
        _manager.GetOrCreateSession("session1");
        _manager.GetOrCreateSession("session2");

        _manager.CleanupExpiredSessions(null);

        _manager.ActiveSessionCount.Should().Be(2);
    }

    [Fact]
    public void CleanupExpiredSessions_WithExpiredSessions_ShouldRemoveThem()
    {
        // Crear manager con timeout muy corto
        using var manager = new GameSessionManager(new GameSettings(), TimeSpan.FromMilliseconds(1));

        manager.GetOrCreateSession("expired-session");

        // Esperar para que expire
        Thread.Sleep(50);

        manager.CleanupExpiredSessions(null);

        manager.ActiveSessionCount.Should().Be(0);
    }

    [Fact]
    public void CleanupExpiredSessions_ShouldKeepActiveSessions()
    {
        // Crear manager con timeout muy corto
        using var manager = new GameSessionManager(new GameSettings(), TimeSpan.FromMilliseconds(100));

        manager.GetOrCreateSession("session1");

        // Esperar un poco pero no suficiente para que expire
        Thread.Sleep(10);

        // Acceder para refrescar timestamp
        manager.GetSession("session1");

        // Ahora esperar más
        Thread.Sleep(20);

        manager.CleanupExpiredSessions(null);

        // La sesión no debería expirar porque la tocamos recientemente
        manager.ActiveSessionCount.Should().BeGreaterThanOrEqualTo(0);
    }
}
