import { ActivatedRouteSnapshot, RouterStateSnapshot, provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { UrlTree } from '@angular/router';

import { ACCESS_TOKEN_KEY } from '../../features/auth/services/auth.service';
import { authGuard } from './auth.guard';

function snapshots(url: string): [ActivatedRouteSnapshot, RouterStateSnapshot] {
  return [
    {} as ActivatedRouteSnapshot,
    { url } as RouterStateSnapshot,
  ];
}

describe('authGuard', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
  });

  afterEach(() => localStorage.clear());

  it('lets authenticated users through', () => {
    localStorage.setItem(ACCESS_TOKEN_KEY, 'access-token');
    const [route, state] = snapshots('/profile');

    expect(TestBed.runInInjectionContext(() => authGuard(route, state))).toBe(true);
  });

  it('redirects guests to login with the returnUrl remembered', () => {
    const [route, state] = snapshots('/profile');

    const result = TestBed.runInInjectionContext(() => authGuard(route, state)) as UrlTree;

    expect(result).not.toBe(true);
    expect(result.toString()).toContain('/login');
    expect(result.toString()).toContain('returnUrl');
  });
});
