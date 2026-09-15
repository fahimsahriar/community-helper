import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

import { ProblemDetails } from '../models/problem-details.model';

/**
 * Turns every failed request into an `Error` carrying a message that is safe to
 * show a user, so components never have to inspect `HttpErrorResponse`.
 */
export const errorNormalizationInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: HttpErrorResponse) => throwError(() => new Error(toMessage(error)))),
  );

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
