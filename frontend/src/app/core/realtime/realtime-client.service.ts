import { Injectable, InjectionToken, OnDestroy, inject, signal } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';

import { environment } from '../../../environments/environment';
import { SocketMessage, SocketMessageTypes, SocketStatus } from './socket-message.model';

/** Swapped in tests for a fake socket. */
export const SOCKET_FACTORY = new InjectionToken<(url: string) => WebSocket>('SOCKET_FACTORY', {
  providedIn: 'root',
  factory: () => (url: string) => new WebSocket(url),
});

const RECONNECT_BASE_DELAY_MS = 500;
const RECONNECT_MAX_DELAY_MS = 30_000;

/**
 * Results can arrive before the HTTP 202 that carries the job id does — the two
 * travel on different connections. Buffering recent messages means a subscriber
 * that arrives late still sees its result instead of hanging until it times out.
 */
const REPLAY_BUFFER_SIZE = 100;
const REPLAY_WINDOW_MS = 60_000;

/**
 * Owns the single WebSocket connection: handshake, reconnect, and message fan-out.
 * See docs/adr/0001 for why this is hand-written rather than SignalR.
 */
@Injectable({ providedIn: 'root' })
export class RealtimeClientService implements OnDestroy {
  private readonly createSocket = inject(SOCKET_FACTORY);
  private readonly url = resolveSocketUrl(environment.socketUrl);

  private readonly inbound = new ReplaySubject<SocketMessage>(REPLAY_BUFFER_SIZE, REPLAY_WINDOW_MS);

  private socket: WebSocket | null = null;
  private reconnectAttempt = 0;
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
  private closedDeliberately = false;

  readonly status = signal<SocketStatus>('disconnected');
  readonly connectionId = signal<string | null>(null);

  readonly messages$: Observable<SocketMessage> = this.inbound.asObservable();

  connect(): void {
    if (
      this.socket &&
      (this.socket.readyState === WebSocket.OPEN || this.socket.readyState === WebSocket.CONNECTING)
    ) {
      return;
    }

    this.clearReconnectTimer();
    this.closedDeliberately = false;
    this.status.set(this.reconnectAttempt === 0 ? 'connecting' : 'reconnecting');

    const socket = this.createSocket(this.url);
    this.socket = socket;

    socket.addEventListener('open', () => {
      this.reconnectAttempt = 0;
      this.status.set('connected');
    });

    socket.addEventListener('message', (event: MessageEvent) => this.handleMessage(event));

    // 'error' is always followed by 'close', so reconnect is driven from 'close' alone.
    socket.addEventListener('close', () => this.handleClose());
  }

  disconnect(): void {
    this.closedDeliberately = true;
    this.clearReconnectTimer();
    this.reconnectAttempt = 0;

    this.socket?.close();
    this.socket = null;

    this.connectionId.set(null);
    this.status.set('disconnected');
  }

  ngOnDestroy(): void {
    this.disconnect();
  }

  private handleMessage(event: MessageEvent): void {
    let message: SocketMessage;
    try {
      message = JSON.parse(event.data as string) as SocketMessage;
    } catch {
      // A frame we cannot parse is not worth tearing the connection down for.
      return;
    }

    if (message.type === SocketMessageTypes.connectionEstablished && message.connectionId) {
      this.connectionId.set(message.connectionId);
    }

    this.inbound.next(message);
  }

  private handleClose(): void {
    this.socket = null;
    this.connectionId.set(null);

    if (this.closedDeliberately) {
      this.status.set('disconnected');
      return;
    }

    this.status.set('reconnecting');
    this.scheduleReconnect();
  }

  private scheduleReconnect(): void {
    const backoff = Math.min(
      RECONNECT_MAX_DELAY_MS,
      RECONNECT_BASE_DELAY_MS * 2 ** this.reconnectAttempt,
    );
    // Jitter keeps many tabs from reconnecting in lockstep after a server restart.
    const delay = backoff + Math.random() * 0.3 * backoff;

    this.reconnectAttempt++;
    this.reconnectTimer = setTimeout(() => this.connect(), delay);
  }

  private clearReconnectTimer(): void {
    if (this.reconnectTimer !== null) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }
  }
}

/** Turns a relative socket path into an absolute ws(s):// URL for the current origin. */
function resolveSocketUrl(configured: string): string {
  if (/^wss?:\/\//i.test(configured)) {
    return configured;
  }

  const scheme = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
  const path = configured.startsWith('/') ? configured : `/${configured}`;

  return `${scheme}//${window.location.host}${path}`;
}
