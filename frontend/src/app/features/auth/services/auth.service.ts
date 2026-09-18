import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, finalize, map, of, tap, throwError } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  AuthResponse,
  CurrentUser,
  LoginPayload,
  RegisterPayload,
  UserRole,
  isUserRole,
} from '../models/auth.model';

export const ACCESS_TOKEN_KEY = 'comunify.access_token';
export const REFRESH_TOKEN_KEY = 'comunify.refresh_token';

/**
 * Sole HTTP access point for `/api/auth/*`. Owns token persistence in
 * `localStorage` so NgRx effects and interceptors never touch storage directly.
 * Components must go through the `auth.store`, never call this directly.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return this.getAccessToken() !== null;
  }

  /**
   * Reads the `role` claim from the stored access token without verifying it.
   * Route guards only — the API remains the authority on every request.
   */
  getTokenRole(): UserRole | null {
    const token = this.getAccessToken();
    const segment = token?.split('.')[1];
    if (!segment) {
      return null;
    }
    try {
      const payload = JSON.parse(atob(segment.replace(/-/g, '+').replace(/_/g, '/'))) as unknown;
      if (typeof payload === 'object' && payload !== null && 'role' in payload) {
        const role: unknown = (payload as { readonly role: unknown }).role;
        return isUserRole(role) ? role : null;
      }
      return null;
    } catch {
      return null;
    }
  }

  register(payload: RegisterPayload): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/register`, payload)
      .pipe(tap((response) => this.persist(response)));
  }

  login(payload: LoginPayload): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, payload)
      .pipe(tap((response) => this.persist(response)));
  }

  googleLogin(code: string, role: UserRole): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/google`, { code, role })
      .pipe(tap((response) => this.persist(response)));
  }

  refresh(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token stored.'));
    }
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/refresh`, { refreshToken })
      .pipe(tap((response) => this.persist(response)));
  }

  getCurrentUser(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(`${this.baseUrl}/me`);
  }

  /** Revokes the refresh token remotely (best effort) and always clears local session. */
  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    const remote: Observable<unknown> =
      refreshToken === null
        ? of(undefined)
        : this.http.post<unknown>(`${this.baseUrl}/logout`, { refreshToken });
    return remote.pipe(
      map(() => undefined),
      finalize(() => this.clearSession()),
    );
  }

  clearSession(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  }

  private persist(response: AuthResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
  }
}
