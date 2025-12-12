import { Routes } from '@angular/router';
import { LobbyComponent } from './components/lobby/lobby.component';
import { GameComponent } from './components/game/game.component';
import { VotingComponent } from './components/voting/voting.component';
import { ResultComponent } from './components/result/result.component';
import { LoginComponent } from './components/login/login.component';
import { CategoriesComponent } from './components/categories/categories.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/lobby', pathMatch: 'full' },
  { path: 'lobby', component: LobbyComponent },
  { path: 'game', component: GameComponent },
  { path: 'voting', component: VotingComponent },
  { path: 'result', component: ResultComponent },
  { path: 'login', component: LoginComponent },
  { path: 'categories', component: CategoriesComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/lobby' }
];
