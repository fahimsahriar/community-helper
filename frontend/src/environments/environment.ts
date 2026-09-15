export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  /** Absolute in dev; a relative path is resolved against window.location. */
  socketUrl: 'ws://localhost:5000/ws',
} as const;
