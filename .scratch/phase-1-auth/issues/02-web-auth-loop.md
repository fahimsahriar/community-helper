# 02: Web auth loop

**What to build:** Volunteer and Organization admin can register, log in (email+password and Google), stay logged in with silent refresh, and be gated by route guards on web, so the login loop is demoable end to end in the browser.

**Blocked by:** 01 (Backend identity + profiles API).

**Status:** done

- [x] `/login` and `/register` pages (Angular Material reactive forms, role picker volunteer vs org admin, Google button) dispatch NgRx `auth.store` actions through `AuthService`
- [x] JWT attach interceptor and 401 silent-refresh-and-retry interceptor handle all API calls without component-level token logic
- [x] `AuthGuard` redirects unauthenticated users to `/login`; `RoleGuard` sends wrong-role users to `/unauthorized`
- [x] Vitest unit tests for `auth.store` reducer + effects (MockStore) and `AuthService` (`provideHttpClientTesting`) pass

Implemented on `feature/phase-1-auth-spec`: `features/auth/` (model, service, store, GIS code helper, login/register pages, lazy routes), `core/interceptors/auth-attach` + `auth-refresh`, `core/guards/auth` + `role`, `shared/components/unauthorized`, store + interceptors wired in `app.config.ts`. Google button exchanges a GIS code via `POST /api/auth/google`; empty `environment.googleClientId` shows "not configured" until a client ID is provisioned. Full suite: 17 files / 100 tests pass; `ng build` clean.
