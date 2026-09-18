import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { errorNormalizationInterceptor, getHttpStatus } from './error-normalization.interceptor';

describe('errorNormalizationInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorNormalizationInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function messageFor(body: string | object | null, status: number, statusText = 'Error'): Promise<string> {
    return new Promise((resolve) => {
      http.get('/test').subscribe({ error: (err: Error) => resolve(err.message) });
      httpMock.expectOne('/test').flush(body, { status, statusText });
    });
  }

  it('explains an unreachable API when the request never lands', async () => {
    const message = await messageFor(null, 0);
    expect(message).toContain('Could not reach the API');
  });

  it('prefers the Problem Details detail field', async () => {
    const message = await messageFor(
      { title: 'Resource not found.', detail: 'Opportunity 9 was not found.' },
      404,
    );
    expect(message).toBe('Opportunity 9 was not found.');
  });

  it('falls back to the Problem Details title when there is no detail', async () => {
    const message = await messageFor({ title: 'Forbidden.' }, 403);
    expect(message).toBe('Forbidden.');
  });

  it('flattens validation errors into a single message', async () => {
    const message = await messageFor(
      { title: 'Validation failed.', errors: { Title: ['Title is required.'] } },
      400,
    );
    expect(message).toBe('Title is required.');
  });

  it('falls back to the status code when the body is not Problem Details', async () => {
    const message = await messageFor('plain text', 500);
    expect(message).toBe('Request failed with status 500.');
  });

  it('preserves the HTTP status on the normalized error', async () => {
    const status = await new Promise<number | null>((resolve) => {
      http.get('/test').subscribe({ error: (err: Error) => resolve(getHttpStatus(err)) });
      httpMock.expectOne('/test').flush({ title: 'Not found.' }, { status: 404, statusText: 'Not Found' });
    });
    expect(status).toBe(404);
  });

  it('returns null for errors without an HTTP status', () => {
    expect(getHttpStatus(new Error('boom'))).toBeNull();
    expect(getHttpStatus(null)).toBeNull();
  });
});
