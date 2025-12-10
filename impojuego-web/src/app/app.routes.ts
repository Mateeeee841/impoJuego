import { Routes } from '@angular/router';
import { LobbyComponent } from './components/lobby/lobby.component';
import { GameComponent } from './components/game/game.component';
import { VotingComponent } from './components/voting/voting.component';
import { ResultComponent } from './components/result/result.component';

export const routes: Routes = [
  { path: '', redirectTo: '/lobby', pathMatch: 'full' },
  { path: 'lobby', component: LobbyComponent },
  { path: 'game', component: GameComponent },
  { path: 'voting', component: VotingComponent },
  { path: 'result', component: ResultComponent },
  { path: '**', redirectTo: '/lobby' }
];
