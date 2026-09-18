import {
  HttpErrorResponse,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';

import { AuthService } from '../../features/auth/services/auth.service';

/**
 * Silently refreshes the token pair once on a 401, then retries the original
 * request with the new access token. Auth endpoints themselves are never
 * retried (a 401 there means bad credentials, not an expired token).
 *
 * Ordering matters: register this interceptor AFTER the error-normalization
 * interceptor in `app.config.ts` so it still sees the raw `HttpErrorResponse`.
 */
export const authRefreshInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (req.url.includes('/auth/')) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || req.headers.has('X-Retry-After-Refresh')) {
        return throwError(() => error);
      }

      return auth.refresh().pipe(
        switchMap(() => {
          const token = auth.getAccessToken();
          const retry: HttpRequest<unknown> = token
            ? req.clone({
                setHeaders: { Authorization: `Bearer ${token}`, 'X-Retry-After-Refresh': 'true' },
              })
            : req.clone({ setHeaders: { 'X-Retry-After-Refresh': 'true' } });
          return next(retry);
        }),
        catchError((refreshError: unknown) => {
          auth.clearSession();
          void router.navigateByUrl('/login');
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};
