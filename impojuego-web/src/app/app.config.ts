import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient()
  ]
};
export const CONFIG = {
  apiUrl: 'https://impojuego-1.onrender.com/api/game'
  //'http://localhost:5000/api/game para correr local
}; 
