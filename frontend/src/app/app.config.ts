import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideEffects } from '@ngrx/effects';
import { provideState, provideStore } from '@ngrx/store';

import { routes } from './app.routes';
import { authAttachInterceptor } from './core/interceptors/auth-attach.interceptor';
import { authRefreshInterceptor } from './core/interceptors/auth-refresh.interceptor';
import { errorNormalizationInterceptor } from './core/interceptors/error-normalization.interceptor';
import { AuthEffects } from './features/auth/store/auth.effects';
import { authFeature } from './features/auth/store/auth.reducer';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideStore(),
    provideState(authFeature),
    provideEffects(AuthEffects),
    // Request order: attach -> normalize -> refresh. Responses flow back in
    // reverse, so the refresh interceptor still sees the raw HttpErrorResponse
    // and the normalization interceptor converts whatever is left afterwards.
    provideHttpClient(
      withFetch(),
      withInterceptors([
        authAttachInterceptor,
        errorNormalizationInterceptor,
        authRefreshInterceptor,
      ]),
    ),
  ],
};
