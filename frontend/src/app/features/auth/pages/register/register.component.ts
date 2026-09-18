import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';

import { requestGoogleCode } from '../../google-code';
import { USER_ROLES, UserRole } from '../../models/auth.model';
import { AuthActions } from '../../store/auth.actions';
import { selectAuthError, selectAuthLoading } from '../../store/auth.selectors';

/** Smart container: owns the registration form and dispatches into `auth.store`. */
@Component({
  selector: 'app-register-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {
  private readonly store = inject(Store);

  protected readonly loading = this.store.selectSignal(selectAuthLoading);
  protected readonly error = this.store.selectSignal(selectAuthError);
  protected readonly googleError = signal<string | null>(null);
  protected readonly googleBusy = signal(false);
  protected readonly roles: readonly UserRole[] = USER_ROLES;

  protected readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['volunteer' as UserRole, [Validators.required]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email, password, role } = this.form.getRawValue();
    this.store.dispatch(AuthActions.register({ email, password, role }));
  }

  protected async signUpWithGoogle(): Promise<void> {
    if (this.googleBusy()) {
      return;
    }
    this.googleBusy.set(true);
    this.googleError.set(null);
    try {
      const code = await requestGoogleCode();
      this.store.dispatch(AuthActions.googleLogin({ code, role: this.form.getRawValue().role }));
    } catch (err: unknown) {
      this.googleError.set(err instanceof Error ? err.message : 'Google sign-up failed.');
    } finally {
      this.googleBusy.set(false);
    }
  }
}
