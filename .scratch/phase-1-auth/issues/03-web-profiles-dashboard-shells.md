# 03: Web profiles + dashboard shells

**What to build:** Logged-in Volunteer can complete and edit their skills/availability/causes/location profile and Organization admin can complete and edit their Organization profile with pending badge, so profiles are demoable on web behind the auth guards.

**Blocked by:** 02 (Web auth loop).

**Status:** done

- [x] Volunteer profile setup flow (skills picker, availability, causes, location) and edit view persist via profile endpoints
- [x] Organization registration form (name, type, cause tags, location, description) shows pending-verified badge when `verified_at` is null
- [x] `/profile` volunteer shell (hours summary placeholder) and `/org/dashboard` org shell load only for the correct role
- [x] Vitest component tests via DOM + `data-testid` selectors and Playwright smoke of setup flow pass

Implemented on `feature/phase-1-auth-spec`: `features/profiles/` (models, `VolunteerProfileService.getMineOrNull/create/update`, `OrganizationService.getMineOrNull/register/update`, dumb `volunteer-profile-form` + `organization-form`, smart `profile` + `org-dashboard` pages with signals per OpportunitiesPage prior art, lazy `PROFILES_ROUTES` with `authGuard` + `roleGuard`), 404-means-setup handled in services via `getHttpStatus` (error-normalization interceptor now preserves status, covered by new tests), shared `utils/csv`, `e2e/profile-setup.spec.ts` smoke (register → profile setup → pending badge, needs local API + `npx playwright test`). Full suite: 25 files / 138 tests pass; `ng build` clean.
