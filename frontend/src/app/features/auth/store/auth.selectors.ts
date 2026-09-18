import { createSelector } from '@ngrx/store';

import { authFeature } from './auth.reducer';

export const selectCurrentUser = authFeature.selectUser;
export const selectAuthLoading = authFeature.selectLoading;
export const selectAuthError = authFeature.selectError;

export const selectIsAuthenticated = createSelector(
  selectCurrentUser,
  (user): boolean => user !== null,
);

export const selectUserRole = createSelector(selectCurrentUser, (user) => user?.role ?? null);
