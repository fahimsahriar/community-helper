import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';

import { Opportunity } from '../../models/opportunity.model';

/** Presentational: renders opportunities. No services, no side effects. */
@Component({
  selector: 'app-opportunity-list',
  imports: [DatePipe, MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './opportunity-list.component.html',
  styleUrl: './opportunity-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpportunityListComponent {
  readonly opportunities = input.required<readonly Opportunity[]>();
}
