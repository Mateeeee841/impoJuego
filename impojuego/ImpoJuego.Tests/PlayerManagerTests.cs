using FluentAssertions;
using ImpoJuego.Managers;
using ImpoJuego.Models;

namespace ImpoJuego.Tests;

public class PlayerManagerTests
{
    private PlayerManager _manager;

    public PlayerManagerTests()
    {
        _manager = new PlayerManager();
    }

    [Fact]
    public void RegisterPlayer_WithValidName_ShouldSucceed()
    {
        var (success, message) = _manager.RegisterPlayer("Player1");

        success.Should().BeTrue();
        message.Should().Contain("registrado");
        _manager.Count.Should().Be(1);
    }

    [Fact]
    public void RegisterPlayer_WithEmptyName_ShouldFail()
    {
        var (success, message) = _manager.RegisterPlayer("");

        success.Should().BeFalse();
        message.Should().Contain("vacío");
    }

    [Fact]
    public void RegisterPlayer_WithWhitespaceName_ShouldFail()
    {
        var (success, message) = _manager.RegisterPlayer("   ");

        success.Should().BeFalse();
        message.Should().Contain("vacío");
    }

    [Fact]
    public void RegisterPlayer_WithDuplicateName_ShouldFail()
    {
        _manager.RegisterPlayer("Player1");
        var (success, message) = _manager.RegisterPlayer("Player1");

        success.Should().BeFalse();
        message.Should().Contain("Ya existe");
    }

    [Fact]
    public void RegisterPlayer_WithDuplicateNameDifferentCase_ShouldFail()
    {
        _manager.RegisterPlayer("Player1");
        var (success, message) = _manager.RegisterPlayer("PLAYER1");

        success.Should().BeFalse();
        message.Should().Contain("Ya existe");
    }

    [Fact]
    public void RemovePlayer_WithExistingPlayer_ShouldSucceed()
    {
        _manager.RegisterPlayer("Player1");

        var result = _manager.RemovePlayer("Player1");

        result.Should().BeTrue();
        _manager.Count.Should().Be(0);
    }

    [Fact]
    public void RemovePlayer_WithNonExistingPlayer_ShouldReturnFalse()
    {
        var result = _manager.RemovePlayer("NonExistent");

        result.Should().BeFalse();
    }

    [Fact]
    public void RemovePlayer_IsCaseInsensitive()
    {
        _manager.RegisterPlayer("Player1");

        var result = _manager.RemovePlayer("PLAYER1");

        result.Should().BeTrue();
        _manager.Count.Should().Be(0);
    }

    [Fact]
    public void GetPlayer_WithExistingPlayer_ShouldReturnPlayer()
    {
        _manager.RegisterPlayer("Player1");

        var player = _manager.GetPlayer("Player1");

        player.Should().NotBeNull();
        player!.Name.Should().Be("Player1");
    }

    [Fact]
    public void GetPlayer_WithNonExistingPlayer_ShouldReturnNull()
    {
        var player = _manager.GetPlayer("NonExistent");

        player.Should().BeNull();
    }

    [Fact]
    public void GetPlayer_IsCaseInsensitive()
    {
        _manager.RegisterPlayer("Player1");

        var player = _manager.GetPlayer("PLAYER1");

        player.Should().NotBeNull();
    }

    [Fact]
    public void PlayerExists_WithExistingPlayer_ShouldReturnTrue()
    {
        _manager.RegisterPlayer("Player1");

        _manager.PlayerExists("Player1").Should().BeTrue();
    }

    [Fact]
    public void PlayerExists_WithNonExistingPlayer_ShouldReturnFalse()
    {
        _manager.PlayerExists("NonExistent").Should().BeFalse();
    }

    [Fact]
    public void GetActivePlayers_ShouldExcludeEliminatedPlayers()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");
        var player1 = _manager.GetPlayer("Player1")!;
        player1.Eliminate();

        var activePlayers = _manager.GetActivePlayers();

        activePlayers.Should().HaveCount(1);
        activePlayers[0].Name.Should().Be("Player2");
    }

    [Fact]
    public void ActiveCount_ShouldCountOnlyNonEliminatedPlayers()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");
        _manager.GetPlayer("Player1")!.Eliminate();

        _manager.ActiveCount.Should().Be(1);
    }

    [Fact]
    public void GetPlayersByRole_ShouldReturnOnlyActivePlayersWithRole()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");
        _manager.RegisterPlayer("Player3");

        _manager.GetPlayer("Player1")!.AssignRole(GameRole.Impostor);
        _manager.GetPlayer("Player2")!.AssignRole(GameRole.Impostor);
        _manager.GetPlayer("Player2")!.Eliminate();

        var impostors = _manager.GetPlayersByRole(GameRole.Impostor);

        impostors.Should().HaveCount(1);
        impostors[0].Name.Should().Be("Player1");
    }

    [Fact]
    public void GetAllPlayersByRole_ShouldIncludeEliminatedPlayers()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");

        _manager.GetPlayer("Player1")!.AssignRole(GameRole.Impostor);
        _manager.GetPlayer("Player2")!.AssignRole(GameRole.Impostor);
        _manager.GetPlayer("Player2")!.Eliminate();

        var allImpostors = _manager.GetAllPlayersByRole(GameRole.Impostor);

        allImpostors.Should().HaveCount(2);
    }

    [Fact]
    public void GetActiveImpostors_ShouldReturnOnlyActiveImpostors()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");

        _manager.GetPlayer("Player1")!.AssignRole(GameRole.Impostor);
        _manager.GetPlayer("Player2")!.AssignRole(GameRole.Impostor);
        _manager.GetPlayer("Player2")!.Eliminate();

        var activeImpostors = _manager.GetActiveImpostors();

        activeImpostors.Should().HaveCount(1);
    }

    [Fact]
    public void GetActiveCrewmates_ShouldReturnOnlyActiveCrewmates()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");
        _manager.RegisterPlayer("Player3");

        _manager.GetPlayer("Player1")!.AssignRole(GameRole.Impostor);
        _manager.GetPlayer("Player3")!.Eliminate();

        var activeCrewmates = _manager.GetActiveCrewmates();

        activeCrewmates.Should().HaveCount(1);
        activeCrewmates[0].Name.Should().Be("Player2");
    }

    [Fact]
    public void AssignRoles_ShouldAssignCorrectNumberOfImpostors()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");
        _manager.RegisterPlayer("Player3");
        _manager.RegisterPlayer("Player4");

        var random = new Random(42); // Seed fijo para reproducibilidad
        _manager.AssignRoles(1, random);

        _manager.GetActiveImpostors().Should().HaveCount(1);
        _manager.GetActiveCrewmates().Should().HaveCount(3);
    }

    [Fact]
    public void AssignRoles_WithTwoImpostors_ShouldAssignCorrectly()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");
        _manager.RegisterPlayer("Player3");
        _manager.RegisterPlayer("Player4");
        _manager.RegisterPlayer("Player5");

        var random = new Random(42);
        _manager.AssignRoles(2, random);

        _manager.GetActiveImpostors().Should().HaveCount(2);
        _manager.GetActiveCrewmates().Should().HaveCount(3);
    }

    [Fact]
    public void AssignRoles_ShouldNotAssignMoreImpostorsThanPossible()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");

        var random = new Random(42);
        _manager.AssignRoles(5, random); // Intenta asignar 5 impostores con solo 2 jugadores

        _manager.GetActiveImpostors().Should().HaveCount(1); // Solo debe haber 1
    }

    [Fact]
    public void ResetForNewGame_ShouldResetAllPlayers()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");

        var player1 = _manager.GetPlayer("Player1")!;
        player1.AssignRole(GameRole.Impostor);
        player1.Eliminate();

        _manager.ResetForNewGame();

        player1.Role.Should().Be(GameRole.Crewmate);
        player1.IsEliminated.Should().BeFalse();
    }

    [Fact]
    public void Clear_ShouldRemoveAllPlayers()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");

        _manager.Clear();

        _manager.Count.Should().Be(0);
        _manager.GetPlayer("Player1").Should().BeNull();
    }

    [Fact]
    public void Players_ShouldReturnReadOnlyList()
    {
        _manager.RegisterPlayer("Player1");
        _manager.RegisterPlayer("Player2");

        var players = _manager.Players;

        players.Should().HaveCount(2);
    }
}
