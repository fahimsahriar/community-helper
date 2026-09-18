import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { ACCESS_TOKEN_KEY } from '../../features/auth/services/auth.service';
import { authAttachInterceptor } from './auth-attach.interceptor';

describe('authAttachInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authAttachInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('attaches the bearer token to API calls', () => {
    localStorage.setItem(ACCESS_TOKEN_KEY, 'access-token');
    http.get('/api/opportunities').subscribe();

    const request = httpMock.expectOne('/api/opportunities');
    expect(request.request.headers.get('Authorization')).toBe('Bearer access-token');
    request.flush([]);
  });

  it('leaves the request untouched when signed out', () => {
    http.get('/api/opportunities').subscribe();

    const request = httpMock.expectOne('/api/opportunities');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush([]);
  });

  it('skips the refresh endpoint so a stale token never rides along', () => {
    localStorage.setItem(ACCESS_TOKEN_KEY, 'stale-token');
    http.post('/api/auth/refresh', { refreshToken: 'stored' }).subscribe();

    const request = httpMock.expectOne('/api/auth/refresh');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });
});
