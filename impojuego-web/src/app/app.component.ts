import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
import { GameStateService } from './services/game-state.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'impojuego-web';
  isLoading = true;
  private shakeInterval: any;
  private particleInterval: any;
  private isTouchDevice = false;
  private initialSyncDone = false;

  constructor(
    private gameStateService: GameStateService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    this.isTouchDevice = this.detectTouchDevice();

    if (!this.isTouchDevice) {
      this.initCursor();
    }

    this.initParticles();
    this.initScreenShake();

    // Sincronizar estado del juego al cargar
    await this.syncGameState();

    // Escuchar cambios de ruta para iniciar/detener polling
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.handleRouteChange(event.url);
    });
  }

  private async syncGameState(): Promise<void> {
    try {
      // Cargar estado actual del servidor
      const state = await this.gameStateService.refreshState().toPromise();

      if (state && state.phase !== 'Lobby') {
        // Si hay un juego en progreso, redirigir a la fase correcta
        this.gameStateService.navigateToCurrentPhase(state);
      }
    } catch (err) {
      console.error('Error syncing game state:', err);
    } finally {
      this.isLoading = false;
      this.initialSyncDone = true;
    }
  }

  private handleRouteChange(url: string): void {
    // Iniciar polling en rutas de juego activo
    const activeGameRoutes = ['/game', '/voting'];
    if (activeGameRoutes.some(route => url.startsWith(route))) {
      this.gameStateService.startPolling();
    } else {
      this.gameStateService.stopPolling();
    }
  }

  private detectTouchDevice(): boolean {
    return 'ontouchstart' in window ||
           navigator.maxTouchPoints > 0 ||
           window.matchMedia('(pointer: coarse)').matches;
  }

  ngOnDestroy(): void {
    if (this.shakeInterval) clearInterval(this.shakeInterval);
    if (this.particleInterval) clearInterval(this.particleInterval);
  }

  @HostListener('document:mousemove', ['$event'])
  onMouseMove(e: MouseEvent): void {
    if (this.isTouchDevice) return;
    document.body.style.setProperty('--cursor-x', e.clientX + 'px');
    document.body.style.setProperty('--cursor-y', e.clientY + 'px');
  }

  private initCursor(): void {
    document.body.classList.add('cursor-ready');
  }

  private initParticles(): void {
    const container = document.getElementById('particles-container');
    if (!container) return;

    // Menos partículas en móvil para mejor performance
    const particleCount = this.isTouchDevice ? 5 : 15;
    const particleInterval = this.isTouchDevice ? 4000 : 2000;

    // Crear partículas iniciales
    for (let i = 0; i < particleCount; i++) {
      setTimeout(() => this.createParticle(container), i * 500);
    }

    // Seguir creando partículas
    this.particleInterval = setInterval(() => {
      this.createParticle(container);
    }, particleInterval);
  }

  private createParticle(container: HTMLElement): void {
    const particle = document.createElement('div');
    particle.className = `particle ${Math.random() > 0.5 ? 'red' : 'purple'}`;

    const size = Math.random() * 8 + 4;
    particle.style.width = size + 'px';
    particle.style.height = size + 'px';
    particle.style.left = Math.random() * 100 + '%';
    particle.style.animationDuration = (Math.random() * 15 + 15) + 's';
    particle.style.animationDelay = Math.random() * 5 + 's';

    container.appendChild(particle);

    // Remover después de la animación
    setTimeout(() => {
      if (particle.parentNode) {
        particle.parentNode.removeChild(particle);
      }
    }, 35000);
  }

  private initScreenShake(): void {
    // Vibración aleatoria cada 15-25 segundos
    const scheduleShake = () => {
      const delay = Math.random() * 10000 + 15000; // 15-25 segundos
      this.shakeInterval = setTimeout(() => {
        document.body.classList.add('shaking');
        setTimeout(() => {
          document.body.classList.remove('shaking');
        }, 300);
        scheduleShake();
      }, delay);
    };

    scheduleShake();
  }
}
