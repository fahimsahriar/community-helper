import { authGuard } from '../../core/guards/auth.guard';
import { PROFILES_ROUTES } from './profiles.routes';

describe('PROFILES_ROUTES', () => {
  it('exposes a volunteer-only /profile shell', () => {
    const route = PROFILES_ROUTES.find((candidate) => candidate.path === 'profile');

    expect(route).toBeDefined();
    expect(route?.title).toBe('My profile');
    expect(route?.canActivate).toHaveLength(2);
    expect(route?.canActivate?.[0]).toBe(authGuard);
  });

  it('exposes an org-admin-only /org/dashboard shell', () => {
    const route = PROFILES_ROUTES.find((candidate) => candidate.path === 'org/dashboard');

    expect(route).toBeDefined();
    expect(route?.title).toBe('Organization dashboard');
    expect(route?.canActivate).toHaveLength(2);
    expect(route?.canActivate?.[0]).toBe(authGuard);
  });
});
