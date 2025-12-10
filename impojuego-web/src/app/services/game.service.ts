import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  ApiResponse,
  LobbyStatus,
  GameStarted,
  PlayerRole,
  GameState,
  RoundResult,
  VotingStatus,
  GameEnd,
  Category
} from '../models';
import { CONFIG } from '../../app/app.config'

@Injectable({
  providedIn: 'root'
})
export class GameService {
  //private readonly apiUrl = 'http://localhost:5000/api/game';
  private readonly apiUrl = CONFIG.apiUrl;

  constructor(private http: HttpClient) {}

  // === LOBBY ===

  getLobbyStatus(): Observable<LobbyStatus> {
    return this.http.get<ApiResponse<LobbyStatus>>(`${this.apiUrl}/lobby`)
      .pipe(map(res => res.data!));
  }

  registerPlayer(name: string): Observable<LobbyStatus> {
    return this.http.post<ApiResponse<LobbyStatus>>(`${this.apiUrl}/players`, { name })
      .pipe(map(res => res.data!));
  }

  removePlayer(name: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/players/${encodeURIComponent(name)}`)
      .pipe(map(() => {}));
  }

  // === GAME FLOW ===

  startGame(): Observable<GameStarted> {
    return this.http.post<ApiResponse<GameStarted>>(`${this.apiUrl}/start`, {})
      .pipe(map(res => res.data!));
  }

  getGameState(): Observable<GameState> {
    return this.http.get<ApiResponse<GameState>>(`${this.apiUrl}/state`)
      .pipe(map(res => res.data!));
  }

  revealRole(playerName: string): Observable<PlayerRole> {
    return this.http.post<ApiResponse<PlayerRole>>(`${this.apiUrl}/reveal`, { playerName })
      .pipe(map(res => res.data!));
  }

  startDiscussion(): Observable<GameState> {
    return this.http.post<ApiResponse<GameState>>(`${this.apiUrl}/discussion`, {})
      .pipe(map(res => res.data!));
  }

  // === VOTING ===

  startVoting(): Observable<GameState> {
    return this.http.post<ApiResponse<GameState>>(`${this.apiUrl}/voting`, {})
      .pipe(map(res => res.data!));
  }

  castVote(voterName: string, targetName: string | null): Observable<void> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/vote`, { voterName, targetName })
      .pipe(map(() => {}));
  }

  getVotingStatus(): Observable<VotingStatus> {
    return this.http.get<ApiResponse<VotingStatus>>(`${this.apiUrl}/votes`)
      .pipe(map(res => res.data!));
  }

  tallyVotes(): Observable<RoundResult> {
    return this.http.post<ApiResponse<RoundResult>>(`${this.apiUrl}/tally`, {})
      .pipe(map(res => res.data!));
  }

  // === GAME END ===

  getGameResult(): Observable<GameEnd> {
    return this.http.get<ApiResponse<GameEnd>>(`${this.apiUrl}/result`)
      .pipe(map(res => res.data!));
  }

  resetGame(): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/reset`, {})
      .pipe(map(() => {}));
  }

  fullReset(): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/full-reset`, {})
      .pipe(map(() => {}));
  }

  // === CATEGORIES ===

  getCategories(): Observable<Category[]> {
    return this.http.get<ApiResponse<Category[]>>(`${this.apiUrl}/categories`)
      .pipe(map(res => res.data!));
  }
}
