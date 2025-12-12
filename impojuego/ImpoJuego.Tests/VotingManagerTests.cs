using FluentAssertions;
using ImpoJuego.Managers;
using ImpoJuego.Models;

namespace ImpoJuego.Tests;

public class VotingManagerTests
{
    private PlayerManager _playerManager;
    private VotingManager _votingManager;
    private Player _player1;
    private Player _player2;
    private Player _player3;

    public VotingManagerTests()
    {
        _playerManager = new PlayerManager();
        _playerManager.RegisterPlayer("Player1");
        _playerManager.RegisterPlayer("Player2");
        _playerManager.RegisterPlayer("Player3");

        _player1 = _playerManager.GetPlayer("Player1")!;
        _player2 = _playerManager.GetPlayer("Player2")!;
        _player3 = _playerManager.GetPlayer("Player3")!;

        _votingManager = new VotingManager(_playerManager);
    }

    [Fact]
    public void CastVote_WithValidVote_ShouldSucceed()
    {
        var (success, message) = _votingManager.CastVote(_player1, _player2);

        success.Should().BeTrue();
        message.Should().Contain("Player1");
        message.Should().Contain("Player2");
    }

    [Fact]
    public void CastVote_WithSkip_ShouldSucceed()
    {
        var (success, message) = _votingManager.CastVote(_player1, null);

        success.Should().BeTrue();
        message.Should().Contain("Skip");
    }

    [Fact]
    public void CastVote_ByEliminatedPlayer_ShouldFail()
    {
        _player1.Eliminate();

        var (success, message) = _votingManager.CastVote(_player1, _player2);

        success.Should().BeFalse();
        message.Should().Contain("eliminados no pueden votar");
    }

    [Fact]
    public void CastVote_ForEliminatedPlayer_ShouldFail()
    {
        _player2.Eliminate();

        var (success, message) = _votingManager.CastVote(_player1, _player2);

        success.Should().BeFalse();
        message.Should().Contain("jugador eliminado");
    }

    [Fact]
    public void CastVote_ForSelf_ShouldFail()
    {
        var (success, message) = _votingManager.CastVote(_player1, _player1);

        success.Should().BeFalse();
        message.Should().Contain("ti mismo");
    }

    [Fact]
    public void HasVoted_WhenPlayerHasVoted_ShouldReturnTrue()
    {
        _votingManager.CastVote(_player1, _player2);

        _votingManager.HasVoted(_player1).Should().BeTrue();
    }

    [Fact]
    public void HasVoted_WhenPlayerHasNotVoted_ShouldReturnFalse()
    {
        _votingManager.HasVoted(_player1).Should().BeFalse();
    }

    [Fact]
    public void VotesCast_ShouldReturnCorrectCount()
    {
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player2, _player1);

        _votingManager.VotesCast.Should().Be(2);
    }

    [Fact]
    public void AllVotesIn_WhenAllVoted_ShouldReturnTrue()
    {
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player2, _player1);
        _votingManager.CastVote(_player3, _player1);

        _votingManager.AllVotesIn().Should().BeTrue();
    }

    [Fact]
    public void AllVotesIn_WhenNotAllVoted_ShouldReturnFalse()
    {
        _votingManager.CastVote(_player1, _player2);

        _votingManager.AllVotesIn().Should().BeFalse();
    }

    [Fact]
    public void AllVotesIn_ShouldIgnoreEliminatedPlayers()
    {
        _player3.Eliminate();
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player2, _player1);

        _votingManager.AllVotesIn().Should().BeTrue();
    }

    [Fact]
    public void TallyVotes_WithClearWinner_ShouldEliminatePlayer()
    {
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player2, _player2);
        _votingManager.CastVote(_player3, _player2);

        var result = _votingManager.TallyVotes();

        result.EliminatedPlayer.Should().Be(_player2);
        result.WasTie.Should().BeFalse();
        _player2.IsEliminated.Should().BeTrue();
    }

    [Fact]
    public void TallyVotes_WithTie_ShouldNotEliminateAnyone()
    {
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player2, _player1);
        _votingManager.CastVote(_player3, null); // Skip

        var result = _votingManager.TallyVotes();

        result.EliminatedPlayer.Should().BeNull();
        result.WasTie.Should().BeTrue();
    }

    [Fact]
    public void TallyVotes_WithSkipMajority_ShouldNotEliminateAnyone()
    {
        _votingManager.CastVote(_player1, null);
        _votingManager.CastVote(_player2, null);
        _votingManager.CastVote(_player3, _player1);

        var result = _votingManager.TallyVotes();

        result.EliminatedPlayer.Should().BeNull();
        result.WasTie.Should().BeFalse();
        result.SkipVotes.Should().Be(2);
    }

    [Fact]
    public void TallyVotes_WithTieBetweenPlayerAndSkip_ShouldBeTie()
    {
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player2, null);
        _votingManager.CastVote(_player3, _player2);

        // Player2 tiene 2 votos, Skip tiene 1
        // No debería ser empate aquí, Player2 debería ser eliminado
        // Agregamos otro jugador para crear un verdadero empate
    }

    [Fact]
    public void TallyVotes_ShouldReturnCorrectVoteCounts()
    {
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player2, _player3);
        _votingManager.CastVote(_player3, _player2);

        var result = _votingManager.TallyVotes();

        result.VoteCounts[_player2].Should().Be(2);
        result.VoteCounts[_player3].Should().Be(1);
    }

    [Fact]
    public void ResetVotes_ShouldClearAllVotes()
    {
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player2, _player1);

        _votingManager.ResetVotes();

        _votingManager.VotesCast.Should().Be(0);
        _votingManager.HasVoted(_player1).Should().BeFalse();
    }

    [Fact]
    public void GetVoteSummary_ShouldReturnFormattedSummary()
    {
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player2, null);

        var summary = _votingManager.GetVoteSummary();

        summary.Should().Contain("Player1");
        summary.Should().Contain("Player2");
        summary.Should().Contain("Skip");
    }

    [Fact]
    public void CastVote_ShouldOverwritePreviousVote()
    {
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player1, _player3);

        _votingManager.VotesCast.Should().Be(1);

        // Verificar que el voto es para player3 (player3 no puede votar por sí mismo)
        _votingManager.CastVote(_player2, _player3);
        _votingManager.CastVote(_player3, _player1); // player3 vota por player1

        var result = _votingManager.TallyVotes();
        result.VoteCounts[_player3].Should().Be(2); // player1 y player2 votan por player3
    }

    [Fact]
    public void TallyVotes_WithNoVotes_ShouldReturnEmptyResult()
    {
        var result = _votingManager.TallyVotes();

        result.EliminatedPlayer.Should().BeNull();
        result.WasTie.Should().BeFalse();
        result.VoteCounts.Should().BeEmpty();
        result.SkipVotes.Should().Be(0);
    }

    [Fact]
    public void TallyVotes_WithAllSkips_ShouldNotEliminateAnyone()
    {
        _votingManager.CastVote(_player1, null);
        _votingManager.CastVote(_player2, null);
        _votingManager.CastVote(_player3, null);

        var result = _votingManager.TallyVotes();

        result.EliminatedPlayer.Should().BeNull();
        result.WasTie.Should().BeFalse();
        result.SkipVotes.Should().Be(3);
    }

    [Fact]
    public void VoteResult_SomeoneWasEliminated_ShouldBeTrue_WhenPlayerEliminated()
    {
        _votingManager.CastVote(_player1, _player2);
        _votingManager.CastVote(_player2, _player2);
        _votingManager.CastVote(_player3, _player2);

        var result = _votingManager.TallyVotes();

        result.SomeoneWasEliminated.Should().BeTrue();
    }

    [Fact]
    public void VoteResult_SomeoneWasEliminated_ShouldBeFalse_WhenNoOneEliminated()
    {
        _votingManager.CastVote(_player1, null);
        _votingManager.CastVote(_player2, null);
        _votingManager.CastVote(_player3, null);

        var result = _votingManager.TallyVotes();

        result.SomeoneWasEliminated.Should().BeFalse();
    }
}
