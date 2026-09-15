import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { finalize } from 'rxjs';

import { OpportunityListComponent } from '../../components/opportunity-list/opportunity-list.component';
import { Opportunity } from '../../models/opportunity.model';
import { OpportunitiesService } from '../../services/opportunities.service';

/**
 * Smart container: owns the request to the API and the state it produces.
 * State is held in signals rather than an NgRx feature slice because nothing
 * outside this page reads it — see frontend/CLAUDE.md on lightweight state.
 */
@Component({
  selector: 'app-opportunities-page',
  imports: [MatButtonModule, MatIconModule, MatProgressBarModule, OpportunityListComponent],
  templateUrl: './opportunities-page.component.html',
  styleUrl: './opportunities-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpportunitiesPageComponent {
  private readonly opportunitiesService = inject(OpportunitiesService);

  protected readonly opportunities = signal<readonly Opportunity[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly hasLoaded = signal(false);

  protected loadOpportunities(): void {
    if (this.loading()) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.opportunitiesService
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (opportunities) => {
          this.opportunities.set(opportunities);
          this.hasLoaded.set(true);
        },
        // Messages are already normalized by errorNormalizationInterceptor.
        error: (err: Error) => {
          this.opportunities.set([]);
          this.hasLoaded.set(false);
          this.error.set(err.message);
        },
      });
  }
}
