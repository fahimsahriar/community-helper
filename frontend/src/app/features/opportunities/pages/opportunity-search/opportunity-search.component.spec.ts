import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WritableSignal, signal } from '@angular/core';
import { Observable, Subject, from, of, throwError } from 'rxjs';

import { RealtimeClientService } from '../../../../core/realtime/realtime-client.service';
import { SocketStatus } from '../../../../core/realtime/socket-message.model';
import { Opportunity } from '../../models/opportunity.model';
import {
  OpportunitySearchJobService,
  SearchJobEvent,
} from '../../services/opportunity-search-job.service';
import { OpportunitySearchComponent } from './opportunity-search.component';

const opportunity: Opportunity = {
  id: '1',
  title: 'After-School Maths Tutoring',
  description: 'Tutor secondary students.',
  location: 'Chattogram',
  isRemote: false,
  startsAtUtc: '2026-09-28T15:30:00Z',
};

class FakeRealtimeClient {
  readonly status: WritableSignal<SocketStatus> = signal<SocketStatus>('connected');
  readonly connectionId: WritableSignal<string | null> = signal<string | null>('conn-1');
}

class FakeSearchJobService {
  lastQuery: string | null = null;
  callCount = 0;
  response: Observable<SearchJobEvent> = of<SearchJobEvent>({ kind: 'submitted', jobId: 'job-1' });

  search(query: string): Observable<SearchJobEvent> {
    this.callCount++;
    this.lastQuery = query;
    return this.response;
  }
}

describe('OpportunitySearchComponent', () => {
  let fixture: ComponentFixture<OpportunitySearchComponent>;
  let jobs: FakeSearchJobService;
  let realtime: FakeRealtimeClient;
  let element: HTMLElement;

  beforeEach(async () => {
    jobs = new FakeSearchJobService();
    realtime = new FakeRealtimeClient();

    await TestBed.configureTestingModule({
      imports: [OpportunitySearchComponent],
      providers: [
        { provide: OpportunitySearchJobService, useValue: jobs },
        { provide: RealtimeClientService, useValue: realtime },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(OpportunitySearchComponent);
    fixture.detectChanges();
    element = fixture.nativeElement as HTMLElement;
  });

  function button(): HTMLButtonElement {
    return element.querySelector<HTMLButtonElement>('[data-testid="search-button"]')!;
  }

  function typeQuery(value: string): void {
    const input = element.querySelector<HTMLInputElement>('[data-testid="search-input"]')!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function submit(): void {
    button().click();
    fixture.detectChanges();
  }

  it('shows the socket status and connection id', () => {
    const status = element.querySelector('[data-testid="socket-status"]');
    expect(status?.textContent).toContain('connected');
    expect(element.querySelector('[data-testid="connection-id"]')?.textContent).toContain('conn-1');
  });

  it('disables the search button until the socket is connected', () => {
    realtime.status.set('reconnecting');
    fixture.detectChanges();

    expect(button().disabled).toBe(true);
  });

  it('submits the typed query', () => {
    typeQuery('tutoring');
    submit();

    expect(jobs.callCount).toBe(1);
    expect(jobs.lastQuery).toBe('tutoring');
  });

  it('does not call the service before the button is pressed', () => {
    expect(jobs.callCount).toBe(0);
    expect(element.querySelector('[data-testid="job-status"]')).toBeNull();
  });

  it('renders the results pushed back for the job', () => {
    jobs.response = from<SearchJobEvent[]>([
      { kind: 'submitted', jobId: 'job-1' },
      { kind: 'running', jobId: 'job-1' },
      { kind: 'completed', jobId: 'job-1', results: [opportunity] },
    ]);

    submit();

    expect(element.querySelector('app-opportunity-list')).not.toBeNull();
    expect(element.textContent).toContain('After-School Maths Tutoring');
    expect(element.querySelector('[data-testid="job-status"]')?.textContent).toContain('job-1');
  });

  it('shows the empty state when the job matches nothing', () => {
    jobs.response = from<SearchJobEvent[]>([
      { kind: 'submitted', jobId: 'job-1' },
      { kind: 'completed', jobId: 'job-1', results: [] },
    ]);

    submit();

    expect(element.querySelector('[data-testid="empty-message"]')).not.toBeNull();
    expect(element.querySelector('app-opportunity-list')).toBeNull();
  });

  it('keeps the progress bar up while the job is still running', () => {
    const pending = new Subject<SearchJobEvent>();
    jobs.response = pending;

    submit();
    expect(element.querySelector('[data-testid="loading-bar"]')).not.toBeNull();

    pending.next({ kind: 'completed', jobId: 'job-1', results: [opportunity] });
    pending.complete();
    fixture.detectChanges();

    expect(element.querySelector('[data-testid="loading-bar"]')).toBeNull();
  });

  it('shows the failure reason when the job fails', () => {
    jobs.response = throwError(() => new Error('the matching engine fell over'));

    submit();

    expect(element.querySelector('[data-testid="error-message"]')?.textContent).toContain(
      'the matching engine fell over',
    );
    expect(element.querySelector('app-opportunity-list')).toBeNull();
  });

  it('clears a previous failure when a later search succeeds', () => {
    jobs.response = throwError(() => new Error('boom'));
    submit();
    expect(element.querySelector('[data-testid="error-message"]')).not.toBeNull();

    jobs.response = from<SearchJobEvent[]>([
      { kind: 'submitted', jobId: 'job-2' },
      { kind: 'completed', jobId: 'job-2', results: [opportunity] },
    ]);
    submit();

    expect(element.querySelector('[data-testid="error-message"]')).toBeNull();
    expect(element.textContent).toContain('After-School Maths Tutoring');
  });
});
