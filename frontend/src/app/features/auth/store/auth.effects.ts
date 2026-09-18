import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';

import { AuthService } from '../services/auth.service';
import { AuthActions } from './auth.actions';

/** All auth side effects live here — reducers stay pure, components stay dumb. */
@Injectable()
export class AuthEffects {
  private readonly actions$ = inject(Actions);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.login),
      switchMap(({ email, password }) =>
        this.authService.login({ email, password }).pipe(
          map((response) => AuthActions.loginSuccess({ response })),
          catchError((error: unknown) =>
            of(AuthActions.loginFailure({ error: toMessage(error) })),
          ),
        ),
      ),
    ),
  );

  register$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.register),
      switchMap(({ email, password, role }) =>
        this.authService.register({ email, password, role }).pipe(
          map((response) => AuthActions.registerSuccess({ response })),
          catchError((error: unknown) =>
            of(AuthActions.registerFailure({ error: toMessage(error) })),
          ),
        ),
      ),
    ),
  );

  googleLogin$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.googleLogin),
      switchMap(({ code, role }) =>
        this.authService.googleLogin(code, role).pipe(
          map((response) => AuthActions.googleLoginSuccess({ response })),
          catchError((error: unknown) =>
            of(AuthActions.googleLoginFailure({ error: toMessage(error) })),
          ),
        ),
      ),
    ),
  );

  refresh$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.refresh),
      switchMap(() =>
        this.authService.refresh().pipe(
          map((response) => AuthActions.refreshSuccess({ response })),
          catchError((error: unknown) =>
            of(AuthActions.refreshFailure({ error: toMessage(error) })),
          ),
        ),
      ),
    ),
  );

  loadCurrentUser$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loadCurrentUser),
      switchMap(() =>
        this.authService.getCurrentUser().pipe(
          map((user) => AuthActions.loadCurrentUserSuccess({ user })),
          catchError((error: unknown) =>
            of(AuthActions.loadCurrentUserFailure({ error: toMessage(error) })),
          ),
        ),
      ),
    ),
  );

  logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logout),
      switchMap(() =>
        this.authService.logout().pipe(
          map(() => AuthActions.logoutSuccess()),
          // Remote revocation is best effort — the local session is cleared
          // by the service either way, so still complete the logout.
          catchError(() => of(AuthActions.logoutSuccess())),
        ),
      ),
    ),
  );

  /** A dead refresh token means the session is over — drop it and start over. */
  refreshFailure$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.refreshFailure),
        tap(() => {
          this.authService.clearSession();
          void this.router.navigateByUrl('/login');
        }),
      ),
    { dispatch: false },
  );

  signedIn$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          AuthActions.loginSuccess,
          AuthActions.registerSuccess,
          AuthActions.googleLoginSuccess,
        ),
        tap(() => {
          // The auth guard stashes the original URL here — go back to it.
          const raw: unknown =
            this.router.routerState.snapshot.root.queryParams['returnUrl'];
          const target =
            typeof raw === 'string' && raw.startsWith('/') && !raw.startsWith('//')
              ? raw
              : '/';
          void this.router.navigateByUrl(target);
        }),
      ),
    { dispatch: false },
  );

  signedOut$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logoutSuccess),
        tap(() => {
          void this.router.navigateByUrl('/login');
        }),
      ),
    { dispatch: false },
  );
}

function toMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'Authentication failed. Please try again.';
}
