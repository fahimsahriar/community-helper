import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { EMPTY, Observable, concat, of, throwError } from 'rxjs';
import { filter, mergeMap, switchMap, takeWhile, timeout } from 'rxjs/operators';

import {
  JobAcceptedResponse,
  SocketMessage,
  SocketMessageTypes,
} from '../../../core/realtime/socket-message.model';
import { RealtimeClientService } from '../../../core/realtime/realtime-client.service';
import { environment } from '../../../../environments/environment';
import { Opportunity } from '../models/opportunity.model';

/** Progress of a single search job, in the order the events arrive. */
export type SearchJobEvent =
  | { readonly kind: 'submitted'; readonly jobId: string }
  | { readonly kind: 'running'; readonly jobId: string }
  | { readonly kind: 'completed'; readonly jobId: string; readonly results: readonly Opportunity[] };

/** A job that never reports back should fail rather than hang forever. */
const JOB_TIMEOUT_MS = 30_000;

/**
 * Submits a search over HTTP and resolves it from the socket stream,
 * correlating the two by job id (docs/adr/0001).
 */
@Injectable({ providedIn: 'root' })
export class OpportunitySearchJobService {
  private readonly http = inject(HttpClient);
  private readonly realtime = inject(RealtimeClientService);
  private readonly submitUrl = `${environment.apiUrl}/jobs/opportunity-search`;

  search(query: string): Observable<SearchJobEvent> {
    const connectionId = this.realtime.connectionId();

    if (!connectionId) {
      return throwError(
        () => new Error('Not connected to the server yet. Wait for the connection and try again.'),
      );
    }

    return this.http
      .post<JobAcceptedResponse>(this.submitUrl, { connectionId, query })
      .pipe(
        switchMap(({ jobId }) =>
          concat(
            of<SearchJobEvent>({ kind: 'submitted', jobId }),
            this.resultsFor(jobId),
          ),
        ),
        // Resets on every event, so a slow-but-progressing job is not cut off.
        timeout({
          each: JOB_TIMEOUT_MS,
          with: () => throwError(() => new Error('The job did not report back in time.')),
        }),
      );
  }

  private resultsFor(jobId: string): Observable<SearchJobEvent> {
    return this.realtime.messages$.pipe(
      filter((message) => message.jobId === jobId),
      mergeMap((message) => this.toEvent(message, jobId)),
      // `true` keeps the completing event itself before the stream ends.
      takeWhile((event) => event.kind !== 'completed', true),
    );
  }

  private toEvent(message: SocketMessage, jobId: string): Observable<SearchJobEvent> {
    switch (message.type) {
      case SocketMessageTypes.jobRunning:
        return of<SearchJobEvent>({ kind: 'running', jobId });

      case SocketMessageTypes.jobCompleted:
        return of<SearchJobEvent>({
          kind: 'completed',
          jobId,
          results: (message.result ?? []) as readonly Opportunity[],
        });

      case SocketMessageTypes.jobFailed:
        return throwError(() => new Error(message.error ?? 'The job failed.'));

      default:
        return EMPTY;
    }
  }
}
