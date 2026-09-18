import { environment } from '../../../environments/environment';

interface CodeClientConfig {
  readonly client_id: string;
  readonly scope: string;
  readonly ux_mode: 'popup';
  readonly callback: (response: { readonly code?: string }) => void;
}

interface CodeClient {
  requestCode(): void;
}

interface GoogleIdentityServices {
  readonly accounts: {
    readonly oauth2: {
      initCodeClient(config: CodeClientConfig): CodeClient;
    };
  };
}

function installedGis(): GoogleIdentityServices | null {
  const candidate: unknown = (window as unknown as { readonly google?: unknown }).google;
  if (typeof candidate !== 'object' || candidate === null || !('accounts' in candidate)) {
    return null;
  }
  return candidate as GoogleIdentityServices;
}

function loadGisScript(): Promise<void> {
  if (installedGis()) {
    return Promise.resolve();
  }
  return new Promise((resolve, reject) => {
    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = (): void => resolve();
    script.onerror = (): void => reject(new Error('Could not load Google sign-in.'));
    document.head.appendChild(script);
  });
}

/**
 * Opens the Google consent popup and resolves with the authorization code
 * the backend exchanges at `POST /api/auth/google`. Rejects when Google
 * sign-in is not configured or the popup is dismissed.
 */
export function requestGoogleCode(): Promise<string> {
  if (!environment.googleClientId) {
    return Promise.reject(new Error('Google sign-in is not configured yet.'));
  }
  return loadGisScript().then(
    () =>
      new Promise<string>((resolve, reject) => {
        const oauth2 = installedGis()?.accounts.oauth2;
        if (!oauth2) {
          reject(new Error('Could not load Google sign-in.'));
          return;
        }
        oauth2
          .initCodeClient({
            client_id: environment.googleClientId,
            scope: 'openid email profile',
            ux_mode: 'popup',
            callback: (response): void => {
              if (response.code) {
                resolve(response.code);
              } else {
                reject(new Error('Google sign-in was cancelled.'));
              }
            },
          })
          .requestCode();
      }),
  );
}
