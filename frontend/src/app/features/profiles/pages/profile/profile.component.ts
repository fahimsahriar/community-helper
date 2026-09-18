import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { VolunteerProfileFormComponent } from '../../components/volunteer-profile-form/volunteer-profile-form.component';
import { VolunteerProfile, VolunteerProfilePayload } from '../../models/volunteer-profile.model';
import { VolunteerProfileService } from '../../services/volunteer-profile.service';

/**
 * Smart container: volunteer shell behind `authGuard + roleGuard(['volunteer'])`.
 * A `null` profile from the service means "not set up yet" and switches to
 * setup mode instead of an error. All HTTP lives in the service — this page
 * only owns the signals the template reads.
 */
@Component({
  selector: 'app-profile-page',
  imports: [MatCardModule, MatProgressBarModule, VolunteerProfileFormComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileComponent implements OnInit {
  private readonly profiles = inject(VolunteerProfileService);

  protected readonly profile = signal<VolunteerProfile | null>(null);
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

    this.profiles.getMineOrNull().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.needsSetup.set(profile === null);
        this.loading.set(false);
      },
      // Messages are already normalized by errorNormalizationInterceptor.
      error: (err: Error) => {
        this.error.set(err.message);
        this.loading.set(false);
      },
    });
  }

  protected onSave(payload: VolunteerProfilePayload): void {
    if (this.saving()) {
      return;
    }
    this.saving.set(true);
    this.error.set(null);
    this.saved.set(null);

    const request =
      this.profile() === null
        ? this.profiles.create(payload)
        : this.profiles.update(payload);

    request.subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.needsSetup.set(false);
        this.saved.set('Profile saved.');
        this.saving.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.saving.set(false);
      },
    });
  }
}
