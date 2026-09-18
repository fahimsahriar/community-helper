import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { AuthResponse } from '../models/auth.model';
import { ACCESS_TOKEN_KEY, AuthService, REFRESH_TOKEN_KEY } from './auth.service';

const response: AuthResponse = {
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  expiresAtUtc: '2026-09-19T00:00:00Z',
  user: { id: 'u-1', email: 'a@example.com', role: 'volunteer' },
};

function tokenWithRole(role: string): string {
  const payload = btoa(JSON.stringify({ sub: 'u-1', role })).replace(/\+/g, '-').replace(/\//g, '_');
  return `header.${payload}.signature`;
}

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('registers and persists the token pair', () => {
    let received: AuthResponse | undefined;
    service
      .register({ email: 'a@example.com', password: 'password123', role: 'volunteer' })
      .subscribe((result) => (received = result));

    const request = httpMock.expectOne(`${environment.apiUrl}/auth/register`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'a@example.com',
      password: 'password123',
      role: 'volunteer',
    });

    request.flush(response);
    expect(received).toEqual(response);
    expect(localStorage.getItem(ACCESS_TOKEN_KEY)).toBe('access-token');
    expect(localStorage.getItem(REFRESH_TOKEN_KEY)).toBe('refresh-token');
    expect(service.isAuthenticated()).toBe(true);
  });

  it('logs in and persists the token pair', () => {
    let received: AuthResponse | undefined;
    service
      .login({ email: 'a@example.com', password: 'password123' })
      .subscribe((result) => (received = result));

    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush(response);

    expect(received).toEqual(response);
    expect(service.getAccessToken()).toBe('access-token');
  });

  it('exchanges a Google code with the selected role', () => {
    service.googleLogin('google-code', 'org_admin').subscribe();

    const request = httpMock.expectOne(`${environment.apiUrl}/auth/google`);
    expect(request.request.body).toEqual({ code: 'google-code', role: 'org_admin' });
    request.flush(response);

    expect(service.isAuthenticated()).toBe(true);
  });

  it('refreshes with the stored refresh token', () => {
    localStorage.setItem(ACCESS_TOKEN_KEY, 'stale');
    localStorage.setItem(REFRESH_TOKEN_KEY, 'stored-refresh');

    service.refresh().subscribe();

    const request = httpMock.expectOne(`${environment.apiUrl}/auth/refresh`);
    expect(request.request.body).toEqual({ refreshToken: 'stored-refresh' });
    request.flush({ ...response, accessToken: 'fresh-access' });

    expect(service.getAccessToken()).toBe('fresh-access');
  });

  it('fails refresh without touching HTTP when no refresh token is stored', async () => {
    await expect(
      new Promise((_resolve, reject) => {
        service.refresh().subscribe({ next: () => reject(new Error('should not emit')), error: reject });
      }),
    ).rejects.toThrow('No refresh token stored.');
    httpMock.expectNone(`${environment.apiUrl}/auth/refresh`);
  });

  it('loads the current user', () => {
    let received = null as AuthResponse['user'] | null;
    service.getCurrentUser().subscribe((user) => (received = user));

    httpMock.expectOne(`${environment.apiUrl}/auth/me`).flush(response.user);
    expect(received).toEqual(response.user);
  });

  it('revokes remotely and clears the local session on logout', () => {
    localStorage.setItem(ACCESS_TOKEN_KEY, 'access-token');
    localStorage.setItem(REFRESH_TOKEN_KEY, 'refresh-token');

    let done = false;
    service.logout().subscribe(() => (done = true));

    const request = httpMock.expectOne(`${environment.apiUrl}/auth/logout`);
    expect(request.request.body).toEqual({ refreshToken: 'refresh-token' });
    request.flush(null);

    expect(done).toBe(true);
    expect(service.isAuthenticated()).toBe(false);
  });

  it('completes logout locally when there is nothing to revoke', () => {
    let done = false;
    service.logout().subscribe(() => (done = true));

    httpMock.expectNone(`${environment.apiUrl}/auth/logout`);
    expect(done).toBe(true);
  });

  it('reads the role claim from the stored access token', () => {
    localStorage.setItem(ACCESS_TOKEN_KEY, tokenWithRole('org_admin'));

    expect(service.getTokenRole()).toBe('org_admin');
  });

  it('returns null for the role when signed out or the token is malformed', () => {
    expect(service.getTokenRole()).toBeNull();

    localStorage.setItem(ACCESS_TOKEN_KEY, 'not-a-jwt');
    expect(service.getTokenRole()).toBeNull();

    localStorage.setItem(ACCESS_TOKEN_KEY, tokenWithRole('platform_admin'));
    expect(service.getTokenRole()).toBeNull();
  });
});
