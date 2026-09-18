# 05: Auth hardening + e2e

**What to build:** Phase 1 auth is production-sane: brute-force resistant, strictly validated, and covered end to end on web and mobile, so CI is green and the slice can be demoed without manual setup.

**Blocked by:** 02 (Web auth loop), 03 (Web profiles + dashboard shells), 04 (Mobile auth + onboarding + profile).

**Status:** ready-for-agent

- [ ] Rate limiting on all `/auth/*` endpoints; JWT `aud`/`iss`/`exp` enforced; role matrix reviewed with no escalation path; user content sanitized; no PII/tokens in logs
- [ ] Playwright e2e register → login → view profile passes against local API; Angular CSP/XSS check has no violations
- [ ] Mobile `HoursCubit`-independent auth coverage: notification/FCM setup deferred, but login persistence and deep-link routing verified on emulator
- [ ] `dotnet test`, frontend Vitest, and Flutter tests all pass on a clean branch
