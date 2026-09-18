import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { RouterLink } from '@angular/router';

/** Landing page for users whose role may not access the requested route. */
@Component({
  selector: 'app-unauthorized',
  imports: [MatButtonModule, MatCardModule, RouterLink],
  template: `
    <mat-card class="unauthorized-card">
      <mat-card-header>
        <mat-card-title data-testid="unauthorized-title">Not authorized</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <p data-testid="unauthorized-message">
          Your account is not allowed to view this page. If you joined with the wrong role,
          create an account with the matching role instead.
        </p>
        <a mat-flat-button color="primary" routerLink="/" data-testid="unauthorized-home">
          Back to browsing
        </a>
      </mat-card-content>
    </mat-card>
  `,
  styles: [
    `
      .unauthorized-card {
        max-width: 480px;
        margin: 3rem auto;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UnauthorizedComponent {}
