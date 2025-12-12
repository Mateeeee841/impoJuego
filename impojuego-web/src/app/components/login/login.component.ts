import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  isLoginMode = true;
  email = '';
  password = '';
  loading = false;
  error = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // Si ya está logueado, redirigir
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/lobby']);
    }
  }

  toggleMode(): void {
    this.isLoginMode = !this.isLoginMode;
    this.error = '';
  }

  submit(): void {
    if (!this.email.trim() || !this.password.trim()) {
      this.error = 'Por favor completá todos los campos';
      return;
    }

    this.loading = true;
    this.error = '';

    const authCall = this.isLoginMode
      ? this.authService.login(this.email.trim(), this.password)
      : this.authService.register(this.email.trim(), this.password);

    authCall.subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/lobby']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Error de autenticación';
      }
    });
  }
}
