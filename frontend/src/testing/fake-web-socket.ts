/**
 * Minimal stand-in for the browser WebSocket, driven manually from tests.
 * Injected through SOCKET_FACTORY.
 */
export class FakeWebSocket {
  static instances: FakeWebSocket[] = [];

  static reset(): void {
    FakeWebSocket.instances = [];
  }

  static get last(): FakeWebSocket {
    const socket = FakeWebSocket.instances.at(-1);
    if (!socket) {
      throw new Error('No FakeWebSocket has been created.');
    }
    return socket;
  }

  readyState: number = WebSocket.CONNECTING;
  closeCalled = false;

  private readonly listeners = new Map<string, ((event: unknown) => void)[]>();

  constructor(readonly url: string) {
    FakeWebSocket.instances.push(this);
  }

  addEventListener(type: string, handler: (event: unknown) => void): void {
    const existing = this.listeners.get(type) ?? [];
    this.listeners.set(type, [...existing, handler]);
  }

  close(): void {
    this.closeCalled = true;
    this.readyState = WebSocket.CLOSED;
  }

  // --- test drivers -------------------------------------------------------

  open(): void {
    this.readyState = WebSocket.OPEN;
    this.dispatch('open', {});
  }

  receive(message: unknown): void {
    this.dispatch('message', { data: JSON.stringify(message) });
  }

  receiveRaw(data: string): void {
    this.dispatch('message', { data });
  }

  serverClose(): void {
    this.readyState = WebSocket.CLOSED;
    this.dispatch('close', {});
  }

  private dispatch(type: string, event: unknown): void {
    for (const handler of this.listeners.get(type) ?? []) {
      handler(event);
    }
  }
}

/** Use as the SOCKET_FACTORY value in TestBed providers. */
export const fakeSocketFactory = (url: string): WebSocket =>
  new FakeWebSocket(url) as unknown as WebSocket;
