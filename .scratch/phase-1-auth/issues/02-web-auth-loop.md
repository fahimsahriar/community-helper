# 02: Web auth loop

**What to build:** Volunteer and Organization admin can register, log in (email+password and Google), stay logged in with silent refresh, and be gated by route guards on web, so the login loop is demoable end to end in the browser.

**Blocked by:** 01 (Backend identity + profiles API).

**Status:** ready-for-agent

- [ ] `/login` and `/register` pages (Angular Material reactive forms, role picker volunteer vs org admin, Google button) dispatch NgRx `auth.store` actions through `AuthService`
- [ ] JWT attach interceptor and 401 silent-refresh-and-retry interceptor handle all API calls without component-level token logic
- [ ] `AuthGuard` redirects unauthenticated users to `/login`; `RoleGuard` sends wrong-role users to `/unauthorized`
- [ ] Vitest unit tests for `auth.store` reducer + effects (MockStore) and `AuthService` (`provideHttpClientTesting`) pass
