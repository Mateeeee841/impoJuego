import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap } from 'rxjs';
import { ApiResponse, User, AuthResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = 'https://impojuego-1.onrender.com/api';
  // private readonly baseUrl = 'http://localhost:5000/api'; // Para desarrollo local

  private readonly TOKEN_KEY = 'impojuego_token';
  private readonly USER_KEY = 'impojuego_user';

  private _user = signal<User | null>(this.loadUserFromStorage());

  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => !!this._user());
  readonly isAdmin = computed(() => this._user()?.role === 'Admin');

  constructor(private http: HttpClient) {}

  private loadUserFromStorage(): User | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    return userJson ? JSON.parse(userJson) : null;
  }

  register(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/auth/register`, { email, password })
      .pipe(
        map(res => res.data!),
        tap(data => this.saveAuth(data))
      );
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/auth/login`, { email, password })
      .pipe(
        map(res => res.data!),
        tap(data => this.saveAuth(data))
      );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this._user.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<ApiResponse<User>>(`${this.baseUrl}/auth/me`)
      .pipe(map(res => res.data!));
  }

  private saveAuth(data: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, data.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(data.user));
    this._user.set(data.user);
  }
}
