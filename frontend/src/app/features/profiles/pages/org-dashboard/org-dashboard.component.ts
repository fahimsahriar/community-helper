import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { OrganizationFormComponent } from '../../components/organization-form/organization-form.component';
import { Organization, OrganizationPayload } from '../../models/organization.model';
import { OrganizationService } from '../../services/organization.service';

/**
 * Smart container: org-admin shell behind `authGuard + roleGuard(['org_admin'])`.
 * A `null` organization from the service means "not registered yet" and
 * switches to registration mode. Shows a pending-verified badge while
 * `verifiedAtUtc` is null (Phase 1 defers manual review).
 */
@Component({
  selector: 'app-org-dashboard-page',
  imports: [MatCardModule, MatChipsModule, MatProgressBarModule, OrganizationFormComponent],
  templateUrl: './org-dashboard.component.html',
  styleUrl: './org-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrgDashboardComponent implements OnInit {
  private readonly organizations = inject(OrganizationService);

  protected readonly organization = signal<Organization | null>(null);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly needsSetup = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly saved = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.saved.set(null);

    this.organizations.getMineOrNull().subscribe({
      next: (organization) => {
        this.organization.set(organization);
        this.needsSetup.set(organization === null);
        this.loading.set(false);
      },
      // Messages are already normalized by errorNormalizationInterceptor.
      error: (err: Error) => {
        this.error.set(err.message);
        this.loading.set(false);
      },
    });
  }

  protected onSave(payload: OrganizationPayload): void {
    if (this.saving()) {
      return;
    }
    this.saving.set(true);
    this.error.set(null);
    this.saved.set(null);

    const current = this.organization();
    const request =
      current === null
        ? this.organizations.register(payload)
        : this.organizations.update(current.id, payload);

    request.subscribe({
      next: (organization) => {
        this.organization.set(organization);
        this.needsSetup.set(false);
        this.saved.set('Organization saved.');
        this.saving.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.saving.set(false);
      },
    });
  }
}
