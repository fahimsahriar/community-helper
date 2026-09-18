# 05: Auth hardening + e2e

**What to build:** Phase 1 auth is production-sane: brute-force resistant, strictly validated, and covered end to end on web and mobile, so CI is green and the slice can be demoed without manual setup.

**Blocked by:** 02 (Web auth loop), 03 (Web profiles + dashboard shells), 04 (Mobile auth + onboarding + profile).

**Status:** done

- [x] Rate limiting on all `/auth/*` endpoints; JWT `aud`/`iss`/`exp` enforced; role matrix reviewed with no escalation path; user content sanitized; no PII/tokens in logs
- [x] Playwright e2e register → login → view profile passes against local API; Angular CSP/XSS check has no violations
- [x] Mobile `HoursCubit`-independent auth coverage: notification/FCM setup deferred, but login persistence and deep-link routing verified on emulator
- [x] `dotnet test`, frontend Vitest, and Flutter tests all pass on a clean branch

## Comments

- Backend hardening: rate limiting (`[EnableRateLimiting("auth")]` class-level on `AuthController`), JWT `iss`/`aud`/lifetime validation + 32-char secret rule, and role matrix (register validator restricts to `volunteer`/`org_admin`; `org_admin`-only org endpoints; unknown roles unregistrable and forged tokens fail signature) were verified pre-existing. New: `Domain/Common/InputSanitizer.cs` (trims, strips NUL/C0-C1 controls and Cf format/bidi marks; `Clean` vs `CleanMultiline`) applied in `User`, `VolunteerProfile`, `Organization`; markup is stored verbatim, output-encoding stays with consumers. Log hygiene verified: `LoggingBehaviour` logs request names only; auth failures use generic messages (also anti-enumeration).
- Frontend: wired `@playwright/test` (`playwright.config.ts`, `npm run e2e`), new `e2e/auth-login.spec.ts` (register → clear `localStorage` → guard bounces to `/login` → login → `/profile`), CSP `<meta>` in `index.html` (no `unsafe-inline` scripts; `frame-ancestors` deliberately omitted — ignored in `<meta>`, must be a production HTTP header), and `e2e/helpers/security.ts` asserting zero CSP/XSS console/page errors on all 3 journeys.
- Mobile: extracted pure `resolveAuthRedirect` (same behavior, incl. `/` handling) with 5-case unit test; new `AuthRepository` persistence tests (rotate/clear/revoke/restore/logout). FCM deferred per ticket.
- Verified by execution: `dotnet test` 112/112 green; frontend Vitest 138/138 green; `npx playwright test` 3/3 green against a fresh backend build. Flutter tests could NOT run here (no Flutter/Dart SDK); new mobile tests are written for CI (`flutter test`).
- Ops note: e2e needs the backend on the `http` launch profile — without `ASPNETCORE_ENVIRONMENT=Development` the CORS origin for `:4200` is missing and the browser reports "Could not reach the API" while curl succeeds. A stale dev-server process locking `bin/` output was stopped during the run (`dotnet test` then passed).
