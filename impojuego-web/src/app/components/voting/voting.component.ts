import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { GameService } from '../../services/game.service';
import { GameState, Player, RoundResult } from '../../models';

@Component({
  selector: 'app-voting',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './voting.component.html',
  styleUrl: './voting.component.scss'
})
export class VotingComponent implements OnInit {
  gameState: GameState | null = null;
  currentVoterIndex = 0;
  votes: Map<string, string | null> = new Map(); // voter -> target (null = skip)
  roundResult: RoundResult | null = null;
  loading = false;
  error = '';

  constructor(
    private gameService: GameService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadGameState();
  }

  loadGameState(): void {
    this.gameService.getGameState().subscribe({
      next: (state) => {
        this.gameState = state;
        if (state.phase === 'Finished') {
          this.router.navigate(['/result']);
        } else if (state.phase !== 'Voting') {
          this.router.navigate(['/game']);
        }
      },
      error: () => this.error = 'Error al cargar estado'
    });
  }

  get currentVoter(): Player | null {
    if (!this.gameState) return null;
    return this.gameState.activePlayers[this.currentVoterIndex] || null;
  }

  get votingComplete(): boolean {
    if (!this.gameState) return false;
    return this.votes.size >= this.gameState.activePlayers.length;
  }

  castVote(targetName: string | null): void {
    if (!this.currentVoter || !this.gameState) return;

    this.loading = true;
    const voterName = this.currentVoter.name;

    this.gameService.castVote(voterName, targetName).subscribe({
      next: () => {
        this.votes.set(voterName, targetName);
        this.currentVoterIndex++;
        this.loading = false;

        // Si todos votaron, contar votos
        if (this.currentVoterIndex >= this.gameState!.activePlayers.length) {
          this.tallyVotes();
        }
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al votar';
        this.loading = false;
      }
    });
  }

  tallyVotes(): void {
    this.loading = true;
    this.gameService.tallyVotes().subscribe({
      next: (result) => {
        this.roundResult = result;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al contar votos';
        this.loading = false;
      }
    });
  }

  continueGame(): void {
    if (this.roundResult?.gameStatus !== 'InProgress') {
      this.router.navigate(['/result']);
    } else {
      // Volver a discusión para siguiente ronda
      this.gameService.getGameState().subscribe({
        next: () => this.router.navigate(['/game'])
      });
    }
  }

  getVotePercentage(votes: number): number {
    if (!this.gameState) return 0;
    const total = this.gameState.activePlayers.length;
    return Math.round((votes / total) * 100);
  }
}
