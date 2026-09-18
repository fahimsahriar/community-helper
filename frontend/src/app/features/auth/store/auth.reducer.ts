import { createFeature, createReducer, on } from '@ngrx/store';

import { CurrentUser } from '../models/auth.model';
import { AuthActions } from './auth.actions';

export interface AuthState {
  readonly user: CurrentUser | null;
  readonly loading: boolean;
  readonly error: string | null;
}

export const initialAuthState: AuthState = {
  user: null,
  loading: false,
  error: null,
};

export const authReducer = createReducer(
  initialAuthState,
  on(
    AuthActions.login,
    AuthActions.register,
    AuthActions.googleLogin,
    AuthActions.refresh,
    AuthActions.loadCurrentUser,
    AuthActions.logout,
    (state): AuthState => ({ ...state, loading: true, error: null }),
  ),
  on(
    AuthActions.loginSuccess,
    AuthActions.registerSuccess,
    AuthActions.googleLoginSuccess,
    AuthActions.refreshSuccess,
    (state, { response }): AuthState => ({ ...state, user: response.user, loading: false, error: null }),
  ),
  on(
    AuthActions.loadCurrentUserSuccess,
    (state, { user }): AuthState => ({ ...state, user, loading: false, error: null }),
  ),
  on(AuthActions.logoutSuccess, (): AuthState => ({ ...initialAuthState })),
  on(
    AuthActions.loginFailure,
    AuthActions.registerFailure,
    AuthActions.googleLoginFailure,
    AuthActions.refreshFailure,
    AuthActions.loadCurrentUserFailure,
    (state, { error }): AuthState => ({ ...state, user: null, loading: false, error }),
  ),
);

export const authFeature = createFeature({
  name: 'auth',
  reducer: authReducer,
});
