import { TestBed } from '@angular/core/testing';

import { FakeWebSocket, fakeSocketFactory } from '../../../testing/fake-web-socket';
import { RealtimeClientService, SOCKET_FACTORY } from './realtime-client.service';
import { SocketMessage, SocketMessageTypes } from './socket-message.model';

describe('RealtimeClientService', () => {
  let service: RealtimeClientService;

  beforeEach(() => {
    FakeWebSocket.reset();
    TestBed.configureTestingModule({
      providers: [{ provide: SOCKET_FACTORY, useValue: fakeSocketFactory }],
    });
    service = TestBed.inject(RealtimeClientService);
  });

  afterEach(() => service.disconnect());

  it('reports connecting, then connected once the socket opens', () => {
    expect(service.status()).toBe('disconnected');

    service.connect();
    expect(service.status()).toBe('connecting');

    FakeWebSocket.last.open();
    expect(service.status()).toBe('connected');
  });

  it('captures the connection id from the handshake message', () => {
    service.connect();
    FakeWebSocket.last.open();

    expect(service.connectionId()).toBeNull();

    FakeWebSocket.last.receive({
      type: SocketMessageTypes.connectionEstablished,
      connectionId: 'conn-42',
    });

    expect(service.connectionId()).toBe('conn-42');
  });

  it('does not open a second socket while one is already connecting', () => {
    service.connect();
    service.connect();

    expect(FakeWebSocket.instances.length).toBe(1);
  });

  it('replays recent messages to a subscriber that arrives late', () => {
    // The result can beat the HTTP 202 that carries the job id, so a subscriber
    // that only learns its job id afterwards must still see the message.
    service.connect();
    FakeWebSocket.last.open();
    FakeWebSocket.last.receive({ type: SocketMessageTypes.jobCompleted, jobId: 'job-1' });

    const seen: SocketMessage[] = [];
    service.messages$.subscribe((message) => seen.push(message));

    expect(seen.map((m) => m.jobId)).toEqual(['job-1']);
  });

  it('ignores a frame that is not valid JSON', () => {
    service.connect();
    FakeWebSocket.last.open();

    const seen: SocketMessage[] = [];
    service.messages$.subscribe((message) => seen.push(message));

    FakeWebSocket.last.receiveRaw('<not json>');

    expect(seen).toEqual([]);
    expect(service.status()).toBe('connected');
  });

  it('clears the connection id and reconnects after an unexpected close', () => {
    vi.useFakeTimers();
    try {
      service.connect();
      FakeWebSocket.last.open();
      FakeWebSocket.last.receive({
        type: SocketMessageTypes.connectionEstablished,
        connectionId: 'conn-42',
      });

      FakeWebSocket.last.serverClose();

      expect(service.status()).toBe('reconnecting');
      expect(service.connectionId()).toBeNull();
      expect(FakeWebSocket.instances.length).toBe(1);

      vi.advanceTimersByTime(5_000);

      expect(FakeWebSocket.instances.length).toBe(2);
    } finally {
      vi.useRealTimers();
    }
  });

  it('does not reconnect after a deliberate disconnect', () => {
    vi.useFakeTimers();
    try {
      service.connect();
      FakeWebSocket.last.open();

      service.disconnect();
      vi.advanceTimersByTime(60_000);

      expect(service.status()).toBe('disconnected');
      expect(FakeWebSocket.instances.length).toBe(1);
    } finally {
      vi.useRealTimers();
    }
  });
});
