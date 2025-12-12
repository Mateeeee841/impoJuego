using FluentAssertions;
using ImpoJuego.Managers;
using ImpoJuego.Models;

namespace ImpoJuego.Tests;

public class MenuManagerTests
{
    private GameManager _gameManager;
    private MenuManager _menuManager;

    public MenuManagerTests()
    {
        _gameManager = new GameManager();
        _menuManager = new MenuManager(_gameManager);
    }

    private void SetupGameInProgress()
    {
        _gameManager.RegisterPlayer("Player1");
        _gameManager.RegisterPlayer("Player2");
        _gameManager.RegisterPlayer("Player3");
        _gameManager.StartGame();
    }

    [Fact]
    public void Constructor_ShouldInitialize()
    {
        var menu = new MenuManager(_gameManager);
        menu.Should().NotBeNull();
    }

    [Fact]
    public void ExecuteOption_ResetGame_ShouldResetToLobby()
    {
        SetupGameInProgress();

        var result = _menuManager.ExecuteOption(MenuOption.ResetGame);

        result.Success.Should().BeTrue();
        result.ExecutedAction.Should().Be(MenuOption.ResetGame);
        _gameManager.CurrentPhase.Should().Be(GamePhase.Lobby);
        _gameManager.Players.Count.Should().Be(3); // Jugadores mantenidos
    }

    [Fact]
    public void ExecuteOption_FullReset_ShouldClearEverything()
    {
        SetupGameInProgress();

        var result = _menuManager.ExecuteOption(MenuOption.FullReset);

        result.Success.Should().BeTrue();
        result.ExecutedAction.Should().Be(MenuOption.FullReset);
        _gameManager.CurrentPhase.Should().Be(GamePhase.Lobby);
        _gameManager.Players.Count.Should().Be(0); // Jugadores eliminados
    }

    [Fact]
    public void ExecuteOption_BackToLobby_ShouldResetToLobby()
    {
        SetupGameInProgress();

        var result = _menuManager.ExecuteOption(MenuOption.BackToLobby);

        result.Success.Should().BeTrue();
        result.ExecutedAction.Should().Be(MenuOption.BackToLobby);
        result.Message.Should().Contain("lobby");
        _gameManager.CurrentPhase.Should().Be(GamePhase.Lobby);
    }

    [Fact]
    public void ResetGame_ShouldReturnSuccessResult()
    {
        SetupGameInProgress();

        var result = _menuManager.ResetGame();

        result.Success.Should().BeTrue();
        result.Message.Should().NotBeNullOrEmpty();
        result.ExecutedAction.Should().Be(MenuOption.ResetGame);
    }

    [Fact]
    public void FullReset_ShouldReturnSuccessResult()
    {
        SetupGameInProgress();

        var result = _menuManager.FullReset();

        result.Success.Should().BeTrue();
        result.Message.Should().NotBeNullOrEmpty();
        result.ExecutedAction.Should().Be(MenuOption.FullReset);
    }

    [Fact]
    public void BackToLobby_ShouldReturnSuccessResult()
    {
        SetupGameInProgress();

        var result = _menuManager.BackToLobby();

        result.Success.Should().BeTrue();
        result.Message.Should().NotBeNullOrEmpty();
        result.ExecutedAction.Should().Be(MenuOption.BackToLobby);
    }

    [Fact]
    public void GetAvailableOptions_InLobbyWithNoPlayers_ShouldReturnEmpty()
    {
        var options = _menuManager.GetAvailableOptions();

        options.Should().BeEmpty();
    }

    [Fact]
    public void GetAvailableOptions_InLobbyWithPlayers_ShouldReturnFullReset()
    {
        _gameManager.RegisterPlayer("Player1");

        var options = _menuManager.GetAvailableOptions();

        options.Should().Contain(MenuOption.FullReset);
        options.Should().NotContain(MenuOption.ResetGame);
        options.Should().NotContain(MenuOption.BackToLobby);
    }

    [Fact]
    public void GetAvailableOptions_DuringGame_ShouldReturnAllOptions()
    {
        SetupGameInProgress();

        var options = _menuManager.GetAvailableOptions();

        options.Should().Contain(MenuOption.ResetGame);
        options.Should().Contain(MenuOption.BackToLobby);
        options.Should().Contain(MenuOption.FullReset);
    }

    [Fact]
    public void MenuActionResult_Defaults_ShouldBeCorrect()
    {
        var result = new MenuActionResult();

        result.Success.Should().BeFalse();
        result.Message.Should().BeEmpty();
        result.ExecutedAction.Should().BeNull();
    }

    [Fact]
    public void ExecuteOption_WithInvalidOption_ShouldReturnFailure()
    {
        // Crear una opción de menú inválida casteando un entero fuera de rango
        var invalidOption = (MenuOption)999;

        var result = _menuManager.ExecuteOption(invalidOption);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("no válida");
    }
}
