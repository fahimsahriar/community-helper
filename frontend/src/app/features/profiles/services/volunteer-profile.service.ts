import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, of, throwError } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { getHttpStatus } from '../../../core/interceptors/error-normalization.interceptor';
import { VolunteerProfile, VolunteerProfilePayload } from '../models/volunteer-profile.model';

/**
 * Sole HTTP access point for `/api/volunteer-profiles/me`.
 * Components must go through the profile page (smart container),
 * never call `HttpClient` directly.
 */
@Injectable({ providedIn: 'root' })
export class VolunteerProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/volunteer-profiles`;

  /**
   * Loads the current profile, or `null` when the volunteer has not set one
   * up yet (the API answers 404). All other failures are rethrown.
   */
  getMineOrNull(): Observable<VolunteerProfile | null> {
    return this.http.get<VolunteerProfile>(`${this.baseUrl}/me`).pipe(
      catchError((error: unknown) =>
        getHttpStatus(error) === 404 ? of(null) : throwError(() => error),
      ),
    );
  }

  create(payload: VolunteerProfilePayload): Observable<VolunteerProfile> {
    return this.http.post<VolunteerProfile>(`${this.baseUrl}/me`, payload);
  }

  update(payload: VolunteerProfilePayload): Observable<VolunteerProfile> {
    return this.http.put<VolunteerProfile>(`${this.baseUrl}/me`, payload);
  }
}
