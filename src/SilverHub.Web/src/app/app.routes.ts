import { Routes } from '@angular/router';
import { Home } from './features/home/home';

export const routes: Routes = [
  { path: '', component: Home },
  {
    path: 'heroes',
    loadComponent: () => import('./features/heroes/heroes-list.page').then((m) => m.HeroesListPage),
  },
];
