import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SessionService {
  private readonly STORAGE_KEY = 'impojuego_session_id';

  constructor() {
    this.ensureSessionId();
  }

  /**
   * Obtiene el session ID actual (lo crea si no existe)
   */
  getSessionId(): string {
    let sessionId = localStorage.getItem(this.STORAGE_KEY);
    if (!sessionId) {
      sessionId = this.generateSessionId();
      localStorage.setItem(this.STORAGE_KEY, sessionId);
    }
    return sessionId;
  }

  /**
   * Fuerza la creación de un nuevo session ID (nueva partida completamente nueva)
   */
  resetSession(): string {
    const newId = this.generateSessionId();
    localStorage.setItem(this.STORAGE_KEY, newId);
    return newId;
  }

  /**
   * Genera un UUID v4
   */
  private generateSessionId(): string {
    return crypto.randomUUID();
  }

  private ensureSessionId(): void {
    this.getSessionId();
  }
}
