using FluentAssertions;
using ImpoJuego.Config;

namespace ImpoJuego.Tests;

public class GameSettingsTests
{
    [Fact]
    public void DefaultSettings_ShouldHaveCorrectValues()
    {
        var settings = new GameSettings();

        settings.MinPlayers.Should().Be(3);
        settings.MaxPlayers.Should().Be(20);
        settings.TwoImpostorsProbability.Should().Be(0.04);
        settings.MinPlayersForTwoImpostors.Should().Be(5);
        settings.ImpostorsKnowEachOther.Should().BeTrue();
    }

    [Fact]
    public void GetImpostorCount_WithLessThan5Players_ShouldReturn1()
    {
        var settings = new GameSettings();
        var random = new Random(42);

        for (int i = 0; i < 100; i++)
        {
            var count = settings.GetImpostorCount(random, 4);
            count.Should().Be(1);
        }
    }

    [Fact]
    public void GetImpostorCount_With5OrMorePlayers_CanReturn2()
    {
        var settings = new GameSettings { TwoImpostorsProbability = 1.0 }; // 100% de probabilidad
        var random = new Random(42);

        var count = settings.GetImpostorCount(random, 5);

        count.Should().Be(2);
    }

    [Fact]
    public void GetImpostorCount_WithZeroProbability_ShouldAlwaysReturn1()
    {
        var settings = new GameSettings { TwoImpostorsProbability = 0.0 };
        var random = new Random(42);

        for (int i = 0; i < 100; i++)
        {
            var count = settings.GetImpostorCount(random, 10);
            count.Should().Be(1);
        }
    }

    [Fact]
    public void Settings_ShouldBeModifiable()
    {
        var settings = new GameSettings
        {
            MinPlayers = 4,
            MaxPlayers = 15,
            TwoImpostorsProbability = 0.1,
            MinPlayersForTwoImpostors = 6,
            ImpostorsKnowEachOther = false
        };

        settings.MinPlayers.Should().Be(4);
        settings.MaxPlayers.Should().Be(15);
        settings.TwoImpostorsProbability.Should().Be(0.1);
        settings.MinPlayersForTwoImpostors.Should().Be(6);
        settings.ImpostorsKnowEachOther.Should().BeFalse();
    }
}
