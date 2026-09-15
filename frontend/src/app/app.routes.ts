import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'opportunities',
    loadChildren: () =>
      import('./features/opportunities/opportunities.routes').then((m) => m.OPPORTUNITIES_ROUTES),
  },
  { path: '', redirectTo: 'opportunities', pathMatch: 'full' },
  {
    path: '**',
    loadComponent: () =>
      import('./shared/components/not-found/not-found.component').then((m) => m.NotFoundComponent),
  },
];
