import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { GameService } from '../../services/game.service';
import { GameEnd } from '../../models';

@Component({
  selector: 'app-result',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './result.component.html',
  styleUrl: './result.component.scss'
})
export class ResultComponent implements OnInit {
  result: GameEnd | null = null;
  loading = false;
  error = '';

  constructor(
    private gameService: GameService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadResult();
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
}
