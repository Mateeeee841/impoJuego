import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { GameService } from '../../services/game.service';
import { GameStateService } from '../../services/game-state.service';
import { GameEnd } from '../../models';

@Component({
  selector: 'app-result',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './result.component.html',
  styleUrl: './result.component.scss'
})
export class ResultComponent implements OnInit, OnDestroy {
  result: GameEnd | null = null;
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
      if (state && state.phase === 'Lobby') {
        this.router.navigate(['/lobby']);
      }
    });

    this.loadResult();
  }

  ngOnDestroy(): void {
    if (this.stateSubscription) {
      this.stateSubscription.unsubscribe();
    }
  }

  loadResult(): void {
    this.gameService.getGameResult().subscribe({
      next: (result) => this.result = result,
      error: (err) => {
        this.error = err.error?.message || 'Error al cargar resultado';
        // Si no hay resultado, volver al juego
        setTimeout(() => this.router.navigate(['/game']), 2000);
      }
    });
  }

  playAgain(): void {
    this.loading = true;
    this.gameService.resetGame().subscribe({
      next: () => this.router.navigate(['/lobby']),
      error: () => {
        this.loading = false;
        this.error = 'Error al reiniciar';
      }
    });
  }

  newGame(): void {
    this.loading = true;
    this.gameService.fullReset().subscribe({
      next: () => this.router.navigate(['/lobby']),
      error: () => {
        this.loading = false;
        this.error = 'Error al reiniciar';
      }
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
}
