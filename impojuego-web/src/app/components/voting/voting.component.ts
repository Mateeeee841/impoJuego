import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { GameService } from '../../services/game.service';
import { GameStateService } from '../../services/game-state.service';
import { GameState, Player, RoundResult } from '../../models';

@Component({
  selector: 'app-voting',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './voting.component.html',
  styleUrl: './voting.component.scss'
})
export class VotingComponent implements OnInit, OnDestroy {
  gameState: GameState | null = null;
  currentVoterIndex = 0;
  votes: Map<string, string | null> = new Map(); // voter -> target (null = skip)
  roundResult: RoundResult | null = null;
  loading = false;
  error = '';
  showEndGameConfirm = false;

  private stateSubscription: Subscription | null = null;

  constructor(
    private gameService: GameService,
    private gameStateService: GameStateService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Suscribirse a cambios de estado
    this.stateSubscription = this.gameStateService.getState().subscribe(state => {
      if (state) {
        this.gameState = state;
        this.handlePhaseChange(state);
      }
    });

    this.loadGameState();
  }

  ngOnDestroy(): void {
    if (this.stateSubscription) {
      this.stateSubscription.unsubscribe();
    }
  }

  private handlePhaseChange(state: GameState): void {
    if (state.phase === 'Lobby') {
      this.router.navigate(['/lobby']);
    } else if (state.phase === 'RoleReveal' || state.phase === 'Discussion') {
      this.router.navigate(['/game']);
    } else if (state.phase === 'Finished') {
      this.router.navigate(['/result']);
    }
  }

  loadGameState(): void {
    this.gameStateService.refreshState().subscribe({
      next: (state) => {
        if (state) {
          this.gameState = state;
          this.handlePhaseChange(state);
        }
      },
      error: () => this.error = 'Error al cargar estado'
    });
  }

  // === END GAME ===

  showEndGameModal(): void {
    this.showEndGameConfirm = true;
  }

  cancelEndGame(): void {
    this.showEndGameConfirm = false;
  }

  async confirmEndGame(): Promise<void> {
    this.showEndGameConfirm = false;
    await this.gameStateService.endGame();
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
