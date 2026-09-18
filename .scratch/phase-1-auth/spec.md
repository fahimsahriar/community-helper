# Spec: Phase 1 — Core Profiles & Authentication

Status: ready-for-agent

Source truth: `docs/Comunify_PRD_v1.0.md` + `docs/plan.md` Phase 1 (Weeks 3–6).
`Comunify_PRD_v1.1.docx` verified as Word export of v1.0 (same Postgres/Redis + Jest/Cypress text), not newer.
Respects ADR-0002 (MongoDB, not PostgreSQL) and ADR-0003 (Vitest + Playwright, not Jest + Cypress).

## Problem Statement

A user who wants to volunteer or post opportunities has no identity on the platform. Without registration, login, and a Volunteer or Organization profile, the matching loop (post → search → apply) cannot start.

## Solution

A user registers as a Volunteer or an Organization admin (email+password or Google), logs in to receive a JWT pair, and completes a Volunteer profile (skills, availability, causes, location) or an Organization profile (name, type, cause tags, location, description, pending verification badge). Session persists with silent refresh on web and mobile.

## User Stories

1. As a Volunteer, I want to register with email + password, so that I can access the platform.
2. As a Volunteer, I want to register/login with Google, so that I skip password friction on mobile.
3. As a Volunteer, I want to log in and stay logged in across restarts, so that I don't re-authenticate daily.
4. As a Volunteer, I want my access token silently refreshed on 401, so that my session doesn't interrupt an apply flow.
5. As a Volunteer, I want to log out, so that my tokens are revoked on shared devices.
6. As a Volunteer, I want to create my profile with skills, availability, causes, location, bio, so that Organizations can vet me and I get matched.
7. As a Volunteer, I want to view and edit my profile, so that my skills and availability stay current.
8. As an Organization admin, I want to register my Organization with name, type, cause tags, location, description, so that I can post opportunities.
9. As an Organization admin, I want to see my Organization as pending-verified, so that I know trust review is deferred, not blocking.
10. As an Organization admin, I want to view and edit my Organization profile, so that cause and location stay accurate.
11. As an Organization admin, I want role-gated access to org routes, so that Volunteers cannot reach posting or applicant review.
12. As a returning user, I want unauthenticated route guards to redirect me to login, so that deep links don't land on broken states.
13. As a platform operator, I want validation errors as Problem Details (RFC 7807), so that web and mobile handle them consistently.

## Implementation Decisions

- Roles in scope: Volunteer + org admin only (Q5). `corp_admin` and `platform_admin` deferred; unknown roles get 403 via role guards.
- Auth methods: email+password (bcrypt, min 8 chars) + Google OAuth code exchange only (Q6). Facebook, magic link, 2FA explicitly deferred.
- Token pair: access 15 min, refresh 7 days rotating with reuse detection, stored in MongoDB `refresh_tokens` collection (Q7). `ICurrentUserService` extracts user ID from claims for handlers; handlers never touch HttpContext.
- Org verification minimal (Q8): new Organization has `verified_at = null` = pending badge; no manual review queue in Phase 1.
- Backend seam (existing, preferred): MediatR Commands (`RegisterUser`, `Login`, `Refresh`, `CreateVolunteerProfile`, `UpdateVolunteerProfile`, `RegisterOrganization`, `UpdateOrganization`) + Queries (`GetCurrentUser`, `GetVolunteerProfile`, `GetOrganizationById`) through existing `LoggingBehaviour → ValidationBehaviour → UnhandledExceptionBehaviour` pipeline and global exception middleware. Reuses repository pattern (`IUserRepository`, `IVolunteerProfileRepository`, `IOrganizationRepository`) over `IMongoCollection`, with indexes on `users.email` (unique), `organizations.cause_tags`, `organizations.location`.
- API contract: `POST /auth/register`, `POST /auth/login`, `POST /auth/refresh`, `POST /auth/google`, profile CRUD endpoints; OpenAPI spec is the single source, regenerated NSwag clients after every schema change.
- Frontend seam (existing pattern): new `features/auth/` routes (`/login`, `/register`, `/profile`, `/org/dashboard` shell) + NgRx `auth.store` (Login/Register/Logout actions, reducer, effects via `AuthService`, selectors) + JWT attach + silent-refresh `HttpInterceptorFn` + `AuthGuard`/`RoleGuard` (`CanActivateFn`). No `HttpClient` in components.
- Mobile seam (greenfield, follow plan convention): `AuthBloc` (Login/Register/Logout/TokenRefreshed events; Initial/Loading/Authenticated/Unauthenticated/Failure states) + `AuthRepository` over Dio (JWT attach + 401 refresh) + Hive token persistence + GoRouter redirect + `ProfileCubit`. One BLoC per feature, immutable states.
- Validation: FluentValidation validators for every command; never called manually, only via pipeline.
- Security: rate-limit `/auth/*`, validate `aud`/`iss`/`exp`, sanitize user content before storage, never log PII/tokens (Serilog structured).

## Testing Decisions

- Good tests assert external behavior (HTTP status + DTO, store state transition, BLoC state sequence), not internals (no private-method or Mongo-driver mocking in handler tests).
- Backend: xUnit + Moq handler/validator unit tests (mock repositories, never `IMongoCollection`); `WebApplicationFactory` integration tests for `/auth/register`, `/auth/login`, `/auth/refresh` happy + validation-failure + unauthorized paths, per existing Opportunities test prior art.
- Frontend: Vitest unit tests for `auth.store` reducer + effects (MockStore), service tests with `provideHttpClientTesting`; Playwright e2e register → login → view profile (per ADR-0003).
- Mobile: `bloc_test` for `AuthBloc`, widget tests for login/register screens.
- New seams get tests at the seam boundary; no feature merges without them.

## Out of Scope

- `corp_admin`, `platform_admin`, team invites, ESG reporting (v2/v3).
- Facebook OAuth, magic link, 2FA.
- Manual org verification queue, peer endorsement, document checks.
- Opportunities search/apply loop (Phase 2), hours tracking and exports (Phase 3), messaging/gamification (v2.0), AI matching (v3.0).
- Postgres migration, Jest/Cypress reintroduction (rejected by ADR-0002/0003).

## Further Notes

- Seams proposed above reuse the highest existing seam in each stack (MediatR pipeline, NgRx store + interceptors, BLoC + Dio). Confirm these seams match expectations before `to-tickets`; if mobile wants a different seam (e.g. shared `AuthRepository` interface shape), flag now.
- Next: `to-tickets` splits this into tracer-bullet tickets with blocking edges under `.scratch/phase-1-auth/issues/`, blockers-first.
