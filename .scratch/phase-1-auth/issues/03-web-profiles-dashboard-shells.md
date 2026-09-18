# 03: Web profiles + dashboard shells

**What to build:** Logged-in Volunteer can complete and edit their skills/availability/causes/location profile and Organization admin can complete and edit their Organization profile with pending badge, so profiles are demoable on web behind the auth guards.

**Blocked by:** 02 (Web auth loop).

**Status:** ready-for-agent

- [ ] Volunteer profile setup flow (skills picker, availability, causes, location) and edit view persist via profile endpoints
- [ ] Organization registration form (name, type, cause tags, location, description) shows pending-verified badge when `verified_at` is null
- [ ] `/profile` volunteer shell (hours summary placeholder) and `/org/dashboard` org shell load only for the correct role
- [ ] Vitest component tests via DOM + `data-testid` selectors and Playwright smoke of setup flow pass
