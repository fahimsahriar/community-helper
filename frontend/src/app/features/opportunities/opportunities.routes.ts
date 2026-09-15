import { Routes } from '@angular/router';

export const OPPORTUNITIES_ROUTES: Routes = [
  {
    path: '',
    title: 'Opportunities',
    loadComponent: () =>
      import('./pages/opportunities-page/opportunities-page.component').then(
        (m) => m.OpportunitiesPageComponent,
      ),
  },
  {
    path: 'search',
    title: 'Search opportunities',
    loadComponent: () =>
      import('./pages/opportunity-search/opportunity-search.component').then(
        (m) => m.OpportunitySearchComponent,
      ),
  },
];
