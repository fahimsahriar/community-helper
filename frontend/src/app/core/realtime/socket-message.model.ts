/** The envelope every server-to-client socket message uses (docs/adr/0001). */
export interface SocketMessage {
  readonly type: string;
  readonly connectionId?: string;
  readonly jobId?: string;
  readonly result?: unknown;
  readonly error?: string;
}

export const SocketMessageTypes = {
  connectionEstablished: 'connection.established',
  jobRunning: 'job.running',
  jobCompleted: 'job.completed',
  jobFailed: 'job.failed',
} as const;

export type SocketStatus = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

/** Returned by every job submission endpoint. */
export interface JobAcceptedResponse {
  readonly jobId: string;
}
