import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { WritableSignal, signal } from '@angular/core';
import { ReplaySubject } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { RealtimeClientService } from '../../../core/realtime/realtime-client.service';
import { SocketMessage, SocketMessageTypes } from '../../../core/realtime/socket-message.model';
import { Opportunity } from '../models/opportunity.model';
import { OpportunitySearchJobService, SearchJobEvent } from './opportunity-search-job.service';

const submitUrl = `${environment.apiUrl}/jobs/opportunity-search`;

const opportunity: Opportunity = {
  id: '1',
  title: 'After-School Maths Tutoring',
  description: 'Tutor secondary students.',
  location: 'Chattogram',
  isRemote: false,
  startsAtUtc: '2026-09-28T15:30:00Z',
};

/** Mirrors the real service, including its replay buffer. */
class FakeRealtimeClient {
  readonly connectionId: WritableSignal<string | null> = signal('conn-1');
  readonly inbound = new ReplaySubject<SocketMessage>(100, 60_000);
  readonly messages$ = this.inbound.asObservable();
}

describe('OpportunitySearchJobService', () => {
  let service: OpportunitySearchJobService;
  let realtime: FakeRealtimeClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    realtime = new FakeRealtimeClient();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RealtimeClientService, useValue: realtime },
      ],
    });

    service = TestBed.inject(OpportunitySearchJobService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function collect(query = 'tutoring') {
    const events: SearchJobEvent[] = [];
    let error: Error | undefined;
    let completed = false;

    service.search(query).subscribe({
      next: (event) => events.push(event),
      error: (err: Error) => (error = err),
      complete: () => (completed = true),
    });

    return {
      events,
      get error() {
        return error;
      },
      get completed() {
        return completed;
      },
    };
  }

  it('posts the connection id with the query and reports the job id', () => {
    const run = collect();

    const request = httpMock.expectOne(submitUrl);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ connectionId: 'conn-1', query: 'tutoring' });

    request.flush({ jobId: 'job-1' });

    expect(run.events).toEqual([{ kind: 'submitted', jobId: 'job-1' }]);
  });

  it('reports running, then completes with the pushed results', () => {
    const run = collect();
    httpMock.expectOne(submitUrl).flush({ jobId: 'job-1' });

    realtime.inbound.next({ type: SocketMessageTypes.jobRunning, jobId: 'job-1' });
    realtime.inbound.next({
      type: SocketMessageTypes.jobCompleted,
      jobId: 'job-1',
      result: [opportunity],
    });

    expect(run.events.map((e) => e.kind)).toEqual(['submitted', 'running', 'completed']);
    expect(run.events.at(-1)).toEqual({
      kind: 'completed',
      jobId: 'job-1',
      results: [opportunity],
    });
    expect(run.completed).toBe(true);
  });

  it('still resolves when the result arrives before the HTTP response does', () => {
    // Regression: the socket push and the 202 race, and the push often wins.
    const run = collect();
    const request = httpMock.expectOne(submitUrl);

    realtime.inbound.next({ type: SocketMessageTypes.jobRunning, jobId: 'job-1' });
    realtime.inbound.next({
      type: SocketMessageTypes.jobCompleted,
      jobId: 'job-1',
      result: [opportunity],
    });

    request.flush({ jobId: 'job-1' });

    expect(run.events.at(-1)).toEqual({
      kind: 'completed',
      jobId: 'job-1',
      results: [opportunity],
    });
    expect(run.completed).toBe(true);
  });

  it('ignores messages belonging to another job', () => {
    const run = collect();
    httpMock.expectOne(submitUrl).flush({ jobId: 'job-1' });

    realtime.inbound.next({
      type: SocketMessageTypes.jobCompleted,
      jobId: 'someone-elses-job',
      result: [opportunity],
    });

    expect(run.events.map((e) => e.kind)).toEqual(['submitted']);
    expect(run.completed).toBe(false);
  });

  it('errors with the server-supplied reason when the job fails', () => {
    const run = collect();
    httpMock.expectOne(submitUrl).flush({ jobId: 'job-1' });

    realtime.inbound.next({
      type: SocketMessageTypes.jobFailed,
      jobId: 'job-1',
      error: 'the matching engine fell over',
    });

    expect(run.error?.message).toBe('the matching engine fell over');
  });

  it('treats an empty result as a completed job with no matches', () => {
    const run = collect();
    httpMock.expectOne(submitUrl).flush({ jobId: 'job-1' });

    realtime.inbound.next({ type: SocketMessageTypes.jobCompleted, jobId: 'job-1', result: [] });

    expect(run.events.at(-1)).toEqual({ kind: 'completed', jobId: 'job-1', results: [] });
  });

  it('fails fast without calling the API when the socket is not connected', () => {
    realtime.connectionId.set(null);

    const run = collect();

    expect(run.error?.message).toContain('Not connected');
    httpMock.expectNone(submitUrl);
  });
});
