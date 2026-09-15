import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { RealtimeClientService } from '../../../../core/realtime/realtime-client.service';
import { OpportunityListComponent } from '../../components/opportunity-list/opportunity-list.component';
import { Opportunity } from '../../models/opportunity.model';
import { OpportunitySearchJobService } from '../../services/opportunity-search-job.service';

type SearchPhase = 'idle' | 'submitting' | 'running' | 'done';

/**
 * Demonstrates the async request-reply pattern: the search leaves over HTTP and
 * the result arrives on the socket (docs/adr/0001).
 */
@Component({
  selector: 'app-opportunity-search',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    OpportunityListComponent,
    ReactiveFormsModule,
  ],
  templateUrl: './opportunity-search.component.html',
  styleUrl: './opportunity-search.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpportunitySearchComponent {
  private readonly jobs = inject(OpportunitySearchJobService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly realtime = inject(RealtimeClientService);

  // A FormGroup, not a bare FormControl: (ngSubmit) is an output of
  // FormGroupDirective, so the form needs [formGroup] for submission to fire.
  protected readonly form = new FormGroup({
    query: new FormControl('', { nonNullable: true }),
  });

  protected readonly phase = signal<SearchPhase>('idle');
  protected readonly jobId = signal<string | null>(null);
  protected readonly results = signal<readonly Opportunity[]>([]);
  protected readonly error = signal<string | null>(null);

  protected readonly busy = computed(
    () => this.phase() === 'submitting' || this.phase() === 'running',
  );

  protected readonly canSubmit = computed(
    () => this.realtime.status() === 'connected' && !this.busy(),
  );

  protected submit(): void {
    if (!this.canSubmit()) {
      return;
    }

    this.phase.set('submitting');
    this.error.set(null);
    this.results.set([]);
    this.jobId.set(null);

    this.jobs
      .search(this.form.controls.query.value)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (event) => {
          this.jobId.set(event.jobId);

          if (event.kind === 'running') {
            this.phase.set('running');
          }

          if (event.kind === 'completed') {
            this.results.set(event.results);
            this.phase.set('done');
          }
        },
        error: (err: Error) => {
          this.error.set(err.message);
          this.phase.set('idle');
        },
      });
  }
}
