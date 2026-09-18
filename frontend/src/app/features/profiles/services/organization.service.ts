import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, of, throwError } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { getHttpStatus } from '../../../core/interceptors/error-normalization.interceptor';
import { Organization, OrganizationPayload } from '../models/organization.model';

/**
 * Sole HTTP access point for the Phase 1 org-admin flow
 * (`POST /api/organizations`, `GET /api/organizations/me`,
 * `PUT /api/organizations/{id}`).
 */
@Injectable({ providedIn: 'root' })
export class OrganizationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/organizations`;

  /**
   * Loads the current organization, or `null` when the org admin has not
   * registered one yet (the API answers 404). All other failures are rethrown.
   */
  getMineOrNull(): Observable<Organization | null> {
    return this.http.get<Organization>(`${this.baseUrl}/me`).pipe(
      catchError((error: unknown) =>
        getHttpStatus(error) === 404 ? of(null) : throwError(() => error),
      ),
    );
  }

  register(payload: OrganizationPayload): Observable<Organization> {
    return this.http.post<Organization>(this.baseUrl, payload);
  }

  update(id: string, payload: OrganizationPayload): Observable<Organization> {
    return this.http.put<Organization>(`${this.baseUrl}/${id}`, payload);
  }
}
