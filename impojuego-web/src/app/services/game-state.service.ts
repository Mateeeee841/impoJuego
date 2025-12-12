import { Injectable, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, Subscription, interval, of } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { GameService } from './game.service';
import { GameState, GamePhase } from '../models';

export interface StoredGameState {
  phase: GamePhase;
  category: string;
  playerName: string | null;
  lastUpdated: number;
}

@Injectable({
  providedIn: 'root'
})
export class GameStateService implements OnDestroy {
  private readonly STORAGE_KEY = 'impojuego_game_state';
  private readonly PLAYER_KEY = 'impojuego_player_name';
  private readonly POLL_INTERVAL = 3000; // 3 segundos

  private gameState$ = new BehaviorSubject<GameState | null>(null);
  private isPolling = false;
  private pollSubscription: Subscription | null = null;
  private currentPlayerName: string | null = null;

  constructor(
    private gameService: GameService,
    private router: Router
  ) {
    this.loadStoredState();
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  // === GETTERS ===

  getState(): Observable<GameState | null> {
    return this.gameState$.asObservable();
  }

  getCurrentState(): GameState | null {
    return this.gameState$.value;
  }

  getPlayerName(): string | null {
    if (!this.currentPlayerName) {
      this.currentPlayerName = localStorage.getItem(this.PLAYER_KEY);
    }
    return this.currentPlayerName;
  }

  // === SETTERS ===

  setPlayerName(name: string): void {
    this.currentPlayerName = name;
    localStorage.setItem(this.PLAYER_KEY, name);
  }

  clearPlayerName(): void {
    this.currentPlayerName = null;
    localStorage.removeItem(this.PLAYER_KEY);
  }

  // === STATE MANAGEMENT ===

  /**
   * Carga el estado del servidor y actualiza el BehaviorSubject
   */
  refreshState(): Observable<GameState | null> {
    return this.gameService.getGameState().pipe(
      tap(state => {
        this.gameState$.next(state);
        this.saveStateToStorage(state);
      }),
      catchError(err => {
        console.error('Error refreshing game state:', err);
        return of(null);
      })
    );
  }

  /**
   * Inicia el polling automático del estado
   */
  startPolling(): void {
    if (this.isPolling) return;

    this.isPolling = true;
    this.pollSubscription = interval(this.POLL_INTERVAL).pipe(
      switchMap(() => this.refreshState())
    ).subscribe();
  }

  /**
   * Detiene el polling automático
   */
  stopPolling(): void {
    this.isPolling = false;
    if (this.pollSubscription) {
      this.pollSubscription.unsubscribe();
      this.pollSubscription = null;
    }
  }

  // === NAVIGATION ===

  /**
   * Navega a la ruta correcta según la fase del juego
   */
  navigateToCurrentPhase(state?: GameState | null): void {
    const currentState = state || this.gameState$.value;
    if (!currentState) {
      this.router.navigate(['/lobby']);
      return;
    }

    const routes: Record<GamePhase, string> = {
      'Lobby': '/lobby',
      'RoleReveal': '/game',
      'Discussion': '/game',
      'Voting': '/voting',
      'Finished': '/result'
    };

    const targetRoute = routes[currentState.phase] || '/lobby';
    const currentUrl = this.router.url;

    // Solo navegar si estamos en una ruta de juego diferente
    if (currentUrl !== targetRoute && this.isGameRoute(currentUrl)) {
      this.router.navigate([targetRoute]);
    }
  }

  /**
   * Sincroniza el estado con el servidor y navega si es necesario
   */
  async syncAndNavigate(): Promise<void> {
    try {
      const state = await this.refreshState().toPromise();
      if (state) {
        this.navigateToCurrentPhase(state);
      }
    } catch (err) {
      console.error('Error syncing state:', err);
    }
  }

  // === GAME ACTIONS ===

  /**
   * Termina la partida actual y vuelve al lobby
   */
  async endGame(): Promise<void> {
    this.stopPolling();

    try {
      await this.gameService.fullReset().toPromise();
    } catch (err) {
      console.error('Error ending game:', err);
    }

    this.clearState();
    this.router.navigate(['/lobby']);
  }

  /**
   * Reinicia el juego manteniendo jugadores
   */
  async resetGame(): Promise<void> {
    this.stopPolling();

    try {
      await this.gameService.resetGame().toPromise();
    } catch (err) {
      console.error('Error resetting game:', err);
    }

    this.clearStateKeepPlayer();
    this.router.navigate(['/lobby']);
  }

  // === STORAGE ===

  private loadStoredState(): void {
    const stored = localStorage.getItem(this.STORAGE_KEY);
    if (stored) {
      try {
        const parsed: StoredGameState = JSON.parse(stored);
        // Verificar que no sea muy viejo (más de 4 horas)
        const fourHours = 4 * 60 * 60 * 1000;
        if (Date.now() - parsed.lastUpdated < fourHours) {
          this.currentPlayerName = parsed.playerName;
        } else {
          this.clearState();
        }
      } catch {
        this.clearState();
      }
    }

    // También cargar el nombre del jugador
    this.currentPlayerName = localStorage.getItem(this.PLAYER_KEY);
  }

  private saveStateToStorage(state: GameState): void {
    const stored: StoredGameState = {
      phase: state.phase,
      category: state.category,
      playerName: this.currentPlayerName,
      lastUpdated: Date.now()
    };
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(stored));
  }

  private clearState(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    localStorage.removeItem(this.PLAYER_KEY);
    this.currentPlayerName = null;
    this.gameState$.next(null);
  }

  private clearStateKeepPlayer(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    this.gameState$.next(null);
  }

  // === HELPERS ===

  private isGameRoute(url: string): boolean {
    const gameRoutes = ['/lobby', '/game', '/voting', '/result'];
    return gameRoutes.some(route => url.startsWith(route));
  }

  /**
   * Verifica si el juego está en progreso (no en lobby ni terminado)
   */
  isGameInProgress(): boolean {
    const state = this.gameState$.value;
    return state !== null && state.phase !== 'Lobby' && state.phase !== 'Finished';
  }

  /**
   * Verifica si hay una sesión de juego activa
   */
  hasActiveSession(): boolean {
    return this.gameState$.value !== null;
  }
}
