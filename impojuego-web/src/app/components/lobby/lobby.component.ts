import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { GameService } from '../../services/game.service';
import { LobbyStatus } from '../../models';

@Component({
  selector: 'app-lobby',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lobby.component.html',
  styleUrl: './lobby.component.scss'
})
export class LobbyComponent implements OnInit {
  lobby: LobbyStatus | null = null;
  newPlayerName = '';
  loading = false;
  error = '';

  constructor(
    private gameService: GameService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadLobby();
  }

  loadLobby(): void {
    this.gameService.getLobbyStatus().subscribe({
      next: (lobby) => this.lobby = lobby,
      error: (err) => this.error = 'Error conectando con el servidor'
    });
  }

  addPlayer(): void {
    if (!this.newPlayerName.trim()) return;

    this.loading = true;
    this.error = '';

    this.gameService.registerPlayer(this.newPlayerName.trim()).subscribe({
      next: (lobby) => {
        this.lobby = lobby;
        this.newPlayerName = '';
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al agregar jugador';
        this.loading = false;
      }
    });
  }

  removePlayer(name: string): void {
    this.gameService.removePlayer(name).subscribe({
      next: () => this.loadLobby(),
      error: (err) => this.error = err.error?.message || 'Error al eliminar'
    });
  }

  startGame(): void {
    if (!this.lobby?.canStart) return;

    this.loading = true;
    this.gameService.startGame().subscribe({
      next: () => {
        this.router.navigate(['/game']);
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al iniciar';
        this.loading = false;
      }
    });
  }
}
