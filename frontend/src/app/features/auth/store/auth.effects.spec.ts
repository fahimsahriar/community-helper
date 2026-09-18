import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { provideMockActions } from '@ngrx/effects/testing';
import { Action } from '@ngrx/store';
import { Observable, ReplaySubject, of, throwError } from 'rxjs';

import { AuthResponse } from '../models/auth.model';
import { AuthService } from '../services/auth.service';
import { AuthActions } from './auth.actions';
import { AuthEffects } from './auth.effects';

const user = { id: 'u-1', email: 'a@example.com', role: 'volunteer' } as const;

const response: AuthResponse = {
  accessToken: 'access',
  refreshToken: 'refresh',
  expiresAtUtc: '2026-09-19T00:00:00Z',
  user: { ...user },
};

describe('AuthEffects', () => {
  let actions$: ReplaySubject<Action>;
  let effects: AuthEffects;
  let auth: {
    login: ReturnType<typeof vi.fn>;
    register: ReturnType<typeof vi.fn>;
    googleLogin: ReturnType<typeof vi.fn>;
    refresh: ReturnType<typeof vi.fn>;
    getCurrentUser: ReturnType<typeof vi.fn>;
    logout: ReturnType<typeof vi.fn>;
    clearSession: ReturnType<typeof vi.fn>;
  };
  let router: { navigateByUrl: ReturnType<typeof vi.fn>; routerState: object };

  function emitted<T extends Action>(effect: Observable<Action>): Promise<T[]> {
    return new Promise((resolve, reject) => {
      const seen: T[] = [];
      effect.subscribe({ next: (action) => seen.push(action as T), error: reject });
      setTimeout(() => resolve(seen), 0);
    });
  }

  beforeEach(() => {
    actions$ = new ReplaySubject<Action>(1);
    auth = {
      login: vi.fn(),
      register: vi.fn(),
      googleLogin: vi.fn(),
      refresh: vi.fn(),
      getCurrentUser: vi.fn(),
      logout: vi.fn(),
      clearSession: vi.fn(),
    };
    router = {
      navigateByUrl: vi.fn(),
      routerState: { snapshot: { root: { queryParams: {} } } },
    };

    TestBed.configureTestingModule({
      providers: [
        AuthEffects,
        provideMockActions(() => actions$),
        { provide: AuthService, useValue: auth },
        { provide: Router, useValue: router },
      ],
    });

    effects = TestBed.inject(AuthEffects);
  });

  it('maps a successful login to loginSuccess', async () => {
    auth.login.mockReturnValue(of(response));
    actions$.next(AuthActions.login({ email: 'a@example.com', password: 'password123' }));

    const [action] = await emitted(effects.login$);
    expect(action).toEqual(AuthActions.loginSuccess({ response }));
    expect(auth.login).toHaveBeenCalledWith({ email: 'a@example.com', password: 'password123' });
  });

  it('maps a failed login to loginFailure with the normalized message', async () => {
    auth.login.mockReturnValue(throwError(() => new Error('Invalid credentials.')));
    actions$.next(AuthActions.login({ email: 'a@example.com', password: 'wrong-pass' }));

    const [action] = await emitted<ReturnType<typeof AuthActions.loginFailure>>(effects.login$);
    expect(action).toEqual(AuthActions.loginFailure({ error: 'Invalid credentials.' }));
  });

  it('maps a successful registration to registerSuccess', async () => {
    auth.register.mockReturnValue(of(response));
    actions$.next(
      AuthActions.register({ email: 'a@example.com', password: 'password123', role: 'org_admin' }),
    );

    const [action] = await emitted(effects.register$);
    expect(action).toEqual(AuthActions.registerSuccess({ response }));
  });

  it('maps a failed Google login to googleLoginFailure', async () => {
    auth.googleLogin.mockReturnValue(throwError(() => new Error('Google sign-in was cancelled.')));
    actions$.next(AuthActions.googleLogin({ code: 'code', role: 'volunteer' }));

    const [action] = await emitted(effects.googleLogin$);
    expect(action).toEqual(AuthActions.googleLoginFailure({ error: 'Google sign-in was cancelled.' }));
  });

  it('maps a successful refresh to refreshSuccess', async () => {
    auth.refresh.mockReturnValue(of(response));
    actions$.next(AuthActions.refresh());

    const [action] = await emitted(effects.refresh$);
    expect(action).toEqual(AuthActions.refreshSuccess({ response }));
  });

  it('maps a loaded user to loadCurrentUserSuccess', async () => {
    auth.getCurrentUser.mockReturnValue(of(response.user));
    actions$.next(AuthActions.loadCurrentUser());

    const [action] = await emitted(effects.loadCurrentUser$);
    expect(action).toEqual(AuthActions.loadCurrentUserSuccess({ user: response.user }));
  });

  it('completes the logout even when remote revocation fails', async () => {
    auth.logout.mockReturnValue(throwError(() => new Error('Could not reach the API.')));
    actions$.next(AuthActions.logout());

    const [action] = await emitted(effects.logout$);
    expect(action).toEqual(AuthActions.logoutSuccess());
  });

  it('navigates home after sign-in', async () => {
    actions$.next(AuthActions.loginSuccess({ response }));
    await emitted(effects.signedIn$);

    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });

  it('sends the user back to the guarded returnUrl after sign-in', async () => {
    router.routerState = { snapshot: { root: { queryParams: { returnUrl: '/profile' } } } };
    actions$.next(AuthActions.registerSuccess({ response }));
    await emitted(effects.signedIn$);

    expect(router.navigateByUrl).toHaveBeenCalledWith('/profile');
  });

  it('clears the session and returns to login when refresh dies', async () => {
    actions$.next(AuthActions.refreshFailure({ error: 'Session expired.' }));
    await emitted(effects.refreshFailure$);

    expect(auth.clearSession).toHaveBeenCalled();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
  });

  it('returns to login after logout', async () => {
    actions$.next(AuthActions.logoutSuccess());
    await emitted(effects.signedOut$);

    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
  });
});
