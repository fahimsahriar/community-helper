import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { FakeWebSocket, fakeSocketFactory } from '../testing/fake-web-socket';
import { App } from './app';
import { SOCKET_FACTORY } from './core/realtime/realtime-client.service';

describe('App', () => {
  beforeEach(async () => {
    FakeWebSocket.reset();

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), { provide: SOCKET_FACTORY, useValue: fakeSocketFactory }],
    }).compileComponents();
  });

  it('renders the application shell', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.brand')?.textContent).toContain('CommunityHelper');
    expect(compiled.querySelector('router-outlet')).not.toBeNull();
  });

  it('opens the socket connection on start-up', () => {
    TestBed.createComponent(App).detectChanges();

    expect(FakeWebSocket.instances.length).toBe(1);
  });
});
