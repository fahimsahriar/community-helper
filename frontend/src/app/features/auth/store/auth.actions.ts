import { createActionGroup, emptyProps, props } from '@ngrx/store';

import { AuthResponse, CurrentUser, UserRole } from '../models/auth.model';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    Login: props<{ readonly email: string; readonly password: string }>(),
    'Login Success': props<{ readonly response: AuthResponse }>(),
    'Login Failure': props<{ readonly error: string }>(),
    Register: props<{ readonly email: string; readonly password: string; readonly role: UserRole }>(),
    'Register Success': props<{ readonly response: AuthResponse }>(),
    'Register Failure': props<{ readonly error: string }>(),
    'Google Login': props<{ readonly code: string; readonly role: UserRole }>(),
    'Google Login Success': props<{ readonly response: AuthResponse }>(),
    'Google Login Failure': props<{ readonly error: string }>(),
    Refresh: emptyProps(),
    'Refresh Success': props<{ readonly response: AuthResponse }>(),
    'Refresh Failure': props<{ readonly error: string }>(),
    'Load Current User': emptyProps(),
    'Load Current User Success': props<{ readonly user: CurrentUser }>(),
    'Load Current User Failure': props<{ readonly error: string }>(),
    Logout: emptyProps(),
    'Logout Success': emptyProps(),
  },
});
