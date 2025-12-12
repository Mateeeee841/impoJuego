using FluentAssertions;
using ImpoJuego.Models;

namespace ImpoJuego.Tests;

public class PlayerTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        var player = new Player("TestPlayer");

        player.Name.Should().Be("TestPlayer");
        player.Role.Should().Be(GameRole.Crewmate);
        player.IsEliminated.Should().BeFalse();
        player.RoundsPlayed.Should().Be(0);
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        var player = new Player("  TestPlayer  ");

        player.Name.Should().Be("TestPlayer");
    }

    [Fact]
    public void AssignRole_ShouldChangeRole()
    {
        var player = new Player("TestPlayer");

        player.AssignRole(GameRole.Impostor);

        player.Role.Should().Be(GameRole.Impostor);
    }

    [Fact]
    public void Eliminate_ShouldSetIsEliminatedToTrue()
    {
        var player = new Player("TestPlayer");

        player.Eliminate();

        player.IsEliminated.Should().BeTrue();
    }

    [Fact]
    public void IncrementRounds_ShouldIncreaseRoundsPlayed()
    {
        var player = new Player("TestPlayer");

        player.IncrementRounds();
        player.IncrementRounds();

        player.RoundsPlayed.Should().Be(2);
    }

    [Fact]
    public void Reset_ShouldRestoreDefaults()
    {
        var player = new Player("TestPlayer");
        player.AssignRole(GameRole.Impostor);
        player.Eliminate();
        player.IncrementRounds();

        player.Reset();

        player.Role.Should().Be(GameRole.Crewmate);
        player.IsEliminated.Should().BeFalse();
        player.RoundsPlayed.Should().Be(0);
    }

    [Fact]
    public void NormalizedName_ShouldReturnLowercaseTrimmedName()
    {
        var player = new Player("  TestPlayer  ");

        player.NormalizedName.Should().Be("testplayer");
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        var player = new Player("TestPlayer");

        player.ToString().Should().Be("TestPlayer");
    }
}
