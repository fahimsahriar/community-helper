import { AuthResponse, CurrentUser } from '../models/auth.model';
import { AuthActions } from './auth.actions';
import { authReducer, initialAuthState } from './auth.reducer';

const user: CurrentUser = { id: 'u-1', email: 'a@example.com', role: 'volunteer' };

const response: AuthResponse = {
  accessToken: 'access',
  refreshToken: 'refresh',
  expiresAtUtc: '2026-09-19T00:00:00Z',
  user,
};

describe('authReducer', () => {
  it('starts signed out, idle, and error-free', () => {
    expect(authReducer(undefined, { type: '@@init' })).toEqual(initialAuthState);
  });

  it.each([
    AuthActions.login({ email: 'a@example.com', password: 'password123' }),
    AuthActions.register({ email: 'a@example.com', password: 'password123', role: 'volunteer' }),
    AuthActions.googleLogin({ code: 'code', role: 'volunteer' }),
    AuthActions.refresh(),
    AuthActions.loadCurrentUser(),
    AuthActions.logout(),
  ])('marks loading on %s', (action) => {
    const state = authReducer({ ...initialAuthState, error: 'stale' }, action);

    expect(state).toEqual({ user: null, loading: true, error: null });
  });

  it.each([
    AuthActions.loginSuccess({ response }),
    AuthActions.registerSuccess({ response }),
    AuthActions.googleLoginSuccess({ response }),
    AuthActions.refreshSuccess({ response }),
  ])('stores the user on %s', (action) => {
    const state = authReducer({ ...initialAuthState, loading: true }, action);

    expect(state).toEqual({ user, loading: false, error: null });
  });

  it('stores the user loaded from /me', () => {
    const state = authReducer(
      { ...initialAuthState, loading: true },
      AuthActions.loadCurrentUserSuccess({ user }),
    );

    expect(state).toEqual({ user, loading: false, error: null });
  });

  it('resets to initial state on logout', () => {
    const state = authReducer({ user, loading: true, error: 'stale' }, AuthActions.logoutSuccess());

    expect(state).toEqual(initialAuthState);
  });

  it.each([
    AuthActions.loginFailure({ error: 'Invalid credentials.' }),
    AuthActions.registerFailure({ error: 'Email is taken.' }),
    AuthActions.googleLoginFailure({ error: 'Google sign-in was cancelled.' }),
    AuthActions.refreshFailure({ error: 'Session expired.' }),
    AuthActions.loadCurrentUserFailure({ error: 'Unauthorized.' }),
  ])('clears the user and keeps the message on %s', (action) => {
    const state = authReducer({ user, loading: true, error: null }, action);

    expect(state.user).toBeNull();
    expect(state.loading).toBe(false);
    expect(state.error).toContain('.');
  });
});
