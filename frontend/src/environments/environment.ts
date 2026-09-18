export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  /** Absolute in dev; a relative path is resolved against window.location. */
  socketUrl: 'ws://localhost:5000/ws',
  /** Empty until a Google OAuth client ID is provisioned — the Google button then explains itself. */
  googleClientId: '',
} as const;
