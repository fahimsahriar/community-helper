import { Routes } from '@angular/router';

import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

export const PROFILES_ROUTES: Routes = [
  {
    path: 'profile',
    title: 'My profile',
    loadComponent: () => import('./pages/profile/profile.component').then((m) => m.ProfileComponent),
    canActivate: [authGuard, roleGuard(['volunteer'])],
  },
  {
    path: 'org/dashboard',
    title: 'Organization dashboard',
    loadComponent: () =>
      import('./pages/org-dashboard/org-dashboard.component').then((m) => m.OrgDashboardComponent),
    canActivate: [authGuard, roleGuard(['org_admin'])],
  },
];
