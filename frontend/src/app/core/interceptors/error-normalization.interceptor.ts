import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

import { ProblemDetails } from '../models/problem-details.model';

/**
 * Turns every failed request into an `Error` carrying a message that is safe to
 * show a user, so components never have to inspect `HttpErrorResponse`.
 * The originating HTTP status is preserved on the error (see `getHttpStatus`)
 * so services can branch on it — e.g. 404 means "no profile yet, show setup".
 */
export const errorNormalizationInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: HttpErrorResponse) =>
      throwError(() => withHttpStatus(new Error(toMessage(error)), error.status)),
    ),
  );

/**
 * Reads the HTTP status of a failed request. Understands both the normalized
 * `Error` produced by this interceptor (`httpStatus`) and a raw
 * `HttpErrorResponse` (e.g. in service unit tests, where no interceptor runs).
 * Returns null when the error did not originate from an HTTP failure.
 */
export function getHttpStatus(error: unknown): number | null {
  if (typeof error === 'object' && error !== null) {
    if ('httpStatus' in error) {
      const status: unknown = (error as { readonly httpStatus: unknown }).httpStatus;
      return typeof status === 'number' ? status : null;
    }
    if ('status' in error) {
      const status: unknown = (error as { readonly status: unknown }).status;
      return typeof status === 'number' ? status : null;
    }
  }
  return null;
}

function withHttpStatus(error: Error, status: number): Error {
  (error as { httpStatus?: number }).httpStatus = status;
  return error;
}

function toMessage(error: HttpErrorResponse): string {
  // Status 0 means the request never reached the server (offline, DNS, CORS).
  if (error.status === 0) {
    return 'Could not reach the API. Check that the backend is running.';
  }

  const problem = error.error as ProblemDetails | null;

  if (problem?.errors) {
    const messages = Object.values(problem.errors).flat();
    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  return problem?.detail ?? problem?.title ?? `Request failed with status ${error.status}.`;
}
