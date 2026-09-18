import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { map, take } from 'rxjs';

import { AuthService } from '../../features/auth/services/auth.service';
import { UserRole } from '../../features/auth/models/auth.model';
import { selectCurrentUser } from '../../features/auth/store/auth.selectors';

/**
 * Sends users without one of `allowed` roles to `/unauthorized`.
 * Prefers the store user, falling back to the JWT `role` claim so a page
 * reload (empty store, valid token) still resolves correctly.
 */
export function roleGuard(allowed: readonly UserRole[]): CanActivateFn {
  return () => {
    const store = inject(Store);
    const auth = inject(AuthService);
    const router = inject(Router);

    return store.select(selectCurrentUser).pipe(
      take(1),
      map((user) => user?.role ?? auth.getTokenRole()),
      map((role) =>
        role !== null && allowed.includes(role) ? true : router.createUrlTree(['/unauthorized']),
      ),
    );
  };
}
