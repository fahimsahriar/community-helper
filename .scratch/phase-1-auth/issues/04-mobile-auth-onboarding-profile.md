# 04: Mobile auth + onboarding + profile

**What to build:** Volunteer and Organization admin can register, log in, onboard, and manage their profile on mobile with persisted session, so the full auth journey is demoable on an emulator.

**Blocked by:** 01 (Backend identity + profiles API).

**Status:** ready-for-agent

- [ ] `AuthBloc` (Login/Register/Logout/TokenRefreshed → Initial/Loading/Authenticated/Unauthenticated/Failure) backed by `AuthRepository` over Dio with JWT attach + 401 silent refresh
- [ ] Hive persists tokens across restarts; GoRouter redirects unauthenticated users to `/login`
- [ ] Login, register (role picker), volunteer onboarding (skills, causes, availability, location), and profile view/edit screens work against the Phase 1 API
- [ ] `bloc_test` unit tests for `AuthBloc` and widget tests for login/register screens pass
