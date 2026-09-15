import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [MatButtonModule, RouterLink],
  template: `
    <section class="not-found">
      <h1>Page not found</h1>
      <a matButton="filled" routerLink="/opportunities">Back to opportunities</a>
    </section>
  `,
  styles: `
    .not-found {
      display: grid;
      gap: 1rem;
      justify-items: start;
      padding: 2rem 1rem;
      max-width: 48rem;
      margin: 0 auto;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotFoundComponent {}
