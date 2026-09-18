import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { AuthService } from '../../features/auth/services/auth.service';

/**
 * Attaches `Authorization: Bearer <token>` to every API call.
 * The refresh endpoint is skipped — it authenticates with the refresh
 * token in its body, not with a (possibly expired) access token.
 */
export const authAttachInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.getAccessToken();

  if (!token || req.url.includes('/auth/refresh')) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
