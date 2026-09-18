import { ChangeDetectionStrategy, Component, effect, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { VolunteerProfile, VolunteerProfilePayload } from '../../models/volunteer-profile.model';
import { fromCsv, toCsv } from '../../utils/csv';

/**
 * Presentational: edits a volunteer profile. No services, no HTTP.
 * Skills/causes are comma-separated text inputs to stay keyboard- and
 * screen-reader-friendly; the page container parses them on save.
 */
@Component({
  selector: 'app-volunteer-profile-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './volunteer-profile-form.component.html',
  styleUrl: './volunteer-profile-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VolunteerProfileFormComponent {
  readonly profile = input<VolunteerProfile | null>(null);
  readonly saving = input(false);

  readonly save = output<VolunteerProfilePayload>();

  protected readonly form = inject(FormBuilder).nonNullable.group({
    skillsText: ['', [Validators.required, Validators.maxLength(2000)]],
    availability: ['', [Validators.required, Validators.maxLength(200)]],
    causesText: ['', [Validators.required, Validators.maxLength(2000)]],
    location: ['', [Validators.required, Validators.maxLength(200)]],
    bio: ['', [Validators.required, Validators.maxLength(2000)]],
  });

  constructor() {
    effect(() => {
      const profile = this.profile();
      this.form.patchValue({
        skillsText: profile ? toCsv(profile.skills) : '',
        availability: profile?.availability ?? '',
        causesText: profile ? toCsv(profile.causes) : '',
        location: profile?.location ?? '',
        bio: profile?.bio ?? '',
      });
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.save.emit({
      skills: fromCsv(raw.skillsText),
      availability: raw.availability.trim(),
      causes: fromCsv(raw.causesText),
      location: raw.location.trim(),
      bio: raw.bio.trim(),
    });
  }
}
