import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../features/auth/services/auth.service';
import { authRefreshInterceptor } from './auth-refresh.interceptor';

describe('authRefreshInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let auth: {
    refresh: ReturnType<typeof vi.fn>;
    getAccessToken: ReturnType<typeof vi.fn>;
    clearSession: ReturnType<typeof vi.fn>;
  };
  let router: { navigateByUrl: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    auth = { refresh: vi.fn(), getAccessToken: vi.fn(), clearSession: vi.fn() };
    router = { navigateByUrl: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authRefreshInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
        { provide: Router, useValue: router },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('refreshes once and retries the request with the new token', () => {
    auth.refresh.mockReturnValue(of({ accessToken: 'fresh' }));
    auth.getAccessToken.mockReturnValue('fresh');

    let received: unknown;
    http.get('/api/opportunities').subscribe((body) => (received = body));

    httpMock.expectOne('/api/opportunities').flush('expired', { status: 401, statusText: 'Unauthorized' });
    expect(auth.refresh).toHaveBeenCalledTimes(1);

    const retry = httpMock.expectOne('/api/opportunities');
    expect(retry.request.headers.get('Authorization')).toBe('Bearer fresh');
    expect(retry.request.headers.get('X-Retry-After-Refresh')).toBe('true');
    retry.flush(['opportunity']);

    expect(received).toEqual(['opportunity']);
  });

  it('passes non-401 failures through without refreshing', async () => {
    const failure = await new Promise((resolve) => {
      http.get('/api/opportunities').subscribe({ error: (err: unknown) => resolve(err) });
      httpMock.expectOne('/api/opportunities').flush('nope', { status: 404, statusText: 'Not Found' });
    });

    expect(auth.refresh).not.toHaveBeenCalled();
    expect(failure).toMatchObject({ status: 404 });
  });

  it('never retries auth endpoints — a 401 there means bad credentials', async () => {
    const failure = await new Promise((resolve) => {
      http.post('/api/auth/login', {}).subscribe({ error: (err: unknown) => resolve(err) });
      httpMock.expectOne('/api/auth/login').flush('bad', { status: 401, statusText: 'Unauthorized' });
    });

    expect(auth.refresh).not.toHaveBeenCalled();
    expect(failure).toMatchObject({ status: 401 });
  });

  it('signs out and returns to login when the refresh itself fails', async () => {
    auth.refresh.mockReturnValue(throwError(() => new Error('Session expired.')));

    await new Promise((resolve) => {
      http.get('/api/opportunities').subscribe({ error: (err: unknown) => resolve(err) });
      httpMock.expectOne('/api/opportunities').flush('expired', { status: 401, statusText: 'Unauthorized' });
    });

    expect(auth.clearSession).toHaveBeenCalled();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
  });
});
