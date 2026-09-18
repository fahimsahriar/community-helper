import { ChangeDetectionStrategy, Component, effect, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Organization, OrganizationPayload } from '../../models/organization.model';
import { fromCsv, toCsv } from '../../utils/csv';

/** Presentational: edits an organization profile. No services, no HTTP. */
@Component({
  selector: 'app-organization-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './organization-form.component.html',
  styleUrl: './organization-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationFormComponent {
  readonly organization = input<Organization | null>(null);
  readonly saving = input(false);

  readonly save = output<OrganizationPayload>();

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    type: ['', [Validators.required, Validators.maxLength(100)]],
    causeTagsText: ['', [Validators.required, Validators.maxLength(2000)]],
    location: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(5000)]],
  });

  constructor() {
    effect(() => {
      const organization = this.organization();
      this.form.patchValue({
        name: organization?.name ?? '',
        type: organization?.type ?? '',
        causeTagsText: organization ? toCsv(organization.causeTags) : '',
        location: organization?.location ?? '',
        description: organization?.description ?? '',
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
      name: raw.name.trim(),
      type: raw.type.trim(),
      causeTags: fromCsv(raw.causeTagsText),
      location: raw.location.trim(),
      description: raw.description.trim(),
    });
  }
}
