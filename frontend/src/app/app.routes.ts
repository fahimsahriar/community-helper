import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'opportunities', pathMatch: 'full' },
  {
    path: '',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: '',
    loadChildren: () =>
      import('./features/profiles/profiles.routes').then((m) => m.PROFILES_ROUTES),
  },
  {
    path: 'opportunities',
    loadChildren: () =>
      import('./features/opportunities/opportunities.routes').then((m) => m.OPPORTUNITIES_ROUTES),
  },
  {
    path: 'unauthorized',
    title: 'Not authorized',
    loadComponent: () =>
      import('./shared/components/unauthorized/unauthorized.component').then(
        (m) => m.UnauthorizedComponent,
      ),
  },
  {
    path: '**',
    loadComponent: () =>
      import('./shared/components/not-found/not-found.component').then((m) => m.NotFoundComponent),
  },
];
