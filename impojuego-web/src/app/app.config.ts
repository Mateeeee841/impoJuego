import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { sessionInterceptor } from './interceptors/session.interceptor';
import { authInterceptor } from './interceptors/auth.interceptor';
import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([sessionInterceptor, authInterceptor]))
  ]
};

// URL del API de juego. La base la da environment (dev vs prod).
export const CONFIG = {
  apiUrl: `${environment.apiBaseUrl}/game`
};
