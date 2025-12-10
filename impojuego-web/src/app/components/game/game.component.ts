import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { GameService } from '../../services/game.service';
import { GameState, PlayerRole } from '../../models';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './game.component.html',
  styleUrl: './game.component.scss'
})
export class GameComponent implements OnInit {
  gameState: GameState | null = null;
  currentPlayerName = '';
  revealedRole: PlayerRole | null = null;
  playersRevealed: Set<string> = new Set();
  showRole = false;
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
        if (state.phase === 'Lobby') {
          this.router.navigate(['/lobby']);
        } else if (state.phase === 'Voting') {
          this.router.navigate(['/voting']);
        } else if (state.phase === 'Finished') {
          this.router.navigate(['/result']);
        }
      },
      error: () => this.error = 'Error al cargar estado del juego'
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
