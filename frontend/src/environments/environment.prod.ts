export const environment = {
  production: true,
  // Replaced at deploy time with the real API origin.
  apiUrl: '/api',
  socketUrl: '/ws',
  /** Injected at deploy time when Google sign-in is enabled. */
  googleClientId: '',
} as const;
