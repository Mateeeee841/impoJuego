import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { GameService } from '../../services/game.service';
import { GameStateService } from '../../services/game-state.service';
import { GameState, PlayerRole } from '../../models';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './game.component.html',
  styleUrl: './game.component.scss'
})
export class GameComponent implements OnInit, OnDestroy {
  gameState: GameState | null = null;
  currentPlayerName = '';
  revealedRole: PlayerRole | null = null;
  playersRevealed: Set<string> = new Set();
  showRole = false;
  showConfirmation = false;
  showEndGameConfirm = false;
  selectedPlayer: string | null = null;
  loading = false;
  error = '';

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
    } else if (state.phase === 'Voting') {
      this.router.navigate(['/voting']);
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
      error: () => this.error = 'Error al cargar estado del juego'
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

  // Seleccionar jugador y mostrar confirmación
  selectPlayer(playerName: string): void {
    if (this.playersRevealed.has(playerName.toLowerCase())) return;
    this.selectedPlayer = playerName;
    this.showConfirmation = true;
  }

  // Cancelar la confirmación
  cancelConfirmation(): void {
    this.selectedPlayer = null;
    this.showConfirmation = false;
  }

  // Confirmar y revelar rol
  confirmReveal(): void {
    if (!this.selectedPlayer) return;

    this.loading = true;
    this.error = '';
    this.showConfirmation = false;

    this.gameService.revealRole(this.selectedPlayer).subscribe({
      next: (role) => {
        this.revealedRole = role;
        this.showRole = true;
        this.loading = false;
        this.selectedPlayer = null;
      },
      error: (err) => {
        this.error = err.error?.message || 'Jugador no encontrado';
        this.loading = false;
        this.selectedPlayer = null;
      }
    });
  }

  revealRole(): void {
    if (!this.currentPlayerName.trim()) return;

    this.loading = true;
    this.error = '';

    this.gameService.revealRole(this.currentPlayerName.trim()).subscribe({
      next: (role) => {
        this.revealedRole = role;
        this.showRole = true;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Jugador no encontrado';
        this.loading = false;
      }
    });
  }

  hideRole(): void {
    if (this.revealedRole) {
      this.playersRevealed.add(this.revealedRole.playerName.toLowerCase());
    }
    this.showRole = false;
    this.revealedRole = null;
    this.currentPlayerName = '';
  }

  get allPlayersRevealed(): boolean {
    if (!this.gameState) return false;
    return this.playersRevealed.size >= this.gameState.activePlayers.length;
  }

  startDiscussion(): void {
    this.gameService.startDiscussion().subscribe({
      next: () => this.loadGameState()
    });
  }

  goToVoting(): void {
    this.gameService.startVoting().subscribe({
      next: () => this.router.navigate(['/voting'])
    });
  }
}
