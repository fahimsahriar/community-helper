import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { RealtimeClientService } from './core/realtime/realtime-client.service';

@Component({
  selector: 'app-root',
  imports: [MatToolbarModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  private readonly realtime = inject(RealtimeClientService);

  constructor() {
    // One connection for the whole app, opened before any page needs it.
    this.realtime.connect();
  }
}
