import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { firstValueFrom, isObservable } from 'rxjs';

import { initialAuthState } from '../../features/auth/store/auth.reducer';
import { ACCESS_TOKEN_KEY } from '../../features/auth/services/auth.service';
import { UserRole } from '../../features/auth/models/auth.model';
import { roleGuard } from './role.guard';

function tokenWithRole(role: string): string {
  const payload = btoa(JSON.stringify({ sub: 'u-1', role })).replace(/\+/g, '-').replace(/\//g, '_');
  return `header.${payload}.signature`;
}

describe('roleGuard', () => {
  let store: MockStore;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideMockStore({ initialState: { auth: initialAuthState } })],
    });
    store = TestBed.inject(MockStore);
  });

  afterEach(() => localStorage.clear());

  function run(allowed: readonly UserRole[]): Promise<unknown> {
    const route = {} as ActivatedRouteSnapshot;
    const state = { url: '/org/dashboard' } as RouterStateSnapshot;
    const result = TestBed.runInInjectionContext(() => roleGuard(allowed)(route, state));
    return isObservable(result) ? firstValueFrom(result) : Promise.resolve(result);
  }

  it('lets a store user with an allowed role through', async () => {
    store.setState({
      auth: {
        ...initialAuthState,
        user: { id: 'u-1', email: 'a@example.com', role: 'org_admin' },
      },
    });

    await expect(run(['org_admin'])).resolves.toBe(true);
  });

  it('sends a store user with the wrong role to unauthorized', async () => {
    store.setState({
      auth: {
        ...initialAuthState,
        user: { id: 'u-1', email: 'a@example.com', role: 'volunteer' },
      },
    });

    const result = await run(['org_admin']);
    expect(result).not.toBe(true);
    expect((result as UrlTree).toString()).toContain('/unauthorized');
  });

  it('falls back to the JWT role claim after a reload empties the store', async () => {
    localStorage.setItem(ACCESS_TOKEN_KEY, tokenWithRole('org_admin'));

    await expect(run(['org_admin'])).resolves.toBe(true);
  });

  it('sends guests with no token to unauthorized', async () => {
    const result = await run(['volunteer', 'org_admin']);
    expect(result).not.toBe(true);
    expect((result as UrlTree).toString()).toContain('/unauthorized');
  });
});
