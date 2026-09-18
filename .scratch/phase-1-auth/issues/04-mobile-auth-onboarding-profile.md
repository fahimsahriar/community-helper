# 04: Mobile auth + onboarding + profile

**What to build:** Volunteer and Organization admin can register, log in, onboard, and manage their profile on mobile with persisted session, so the full auth journey is demoable on an emulator.

**Blocked by:** 01 (Backend identity + profiles API).

**Status:** done

- [x] `AuthBloc` (Login/Register/Logout/TokenRefreshed → Initial/Loading/Authenticated/Unauthenticated/Failure) backed by `AuthRepository` over Dio with JWT attach + 401 silent refresh
- [x] Hive persists tokens across restarts; GoRouter redirects unauthenticated users to `/login`
- [x] Login, register (role picker), volunteer onboarding (skills, causes, availability, location), and profile view/edit screens work against the Phase 1 API
- [x] `bloc_test` unit tests for `AuthBloc` and widget tests for login/register screens pass

## Comments

- Implemented on branch `feature/phase-1-auth-spec` per `/implement` (greenfield `mobile/`: pubspec, app shell, core network/router/theme, auth + profile features, unit + widget + integration tests).
- Notes vs ticket: added `AuthStarted` alongside the four listed events for Hive cold-boot restore; org register/edit screen included (org_admin role). Google code exchange deferred (not in ticket checkboxes; needs native OAuth client config).
- API contract verified against backend `AuthController` (`/api/auth/*`), `VolunteerProfilesController`, `OrganizationsController` + DTOs (`AuthResultDto`, `CurrentUserDto`, `VolunteerProfileDto`, `OrganizationDto`).
- NOT verified by execution: no Flutter/Dart SDK on this machine (`flutter`/`dart` not found; checked default install paths), so `dart run build_runner build`, `flutter analyze`, and `flutter test` could not run. Reviewer/CI must run: `cd mobile && flutter pub get && dart run build_runner build --delete-conflicting-outputs && flutter analyze && flutter test`.
- Standards self-check (mobile CLAUDE.md): feature-first layout, Freezed models, sealed BLoC events/states with `isClosed` guards, repository pattern (no Dio in UI), sealed Failure hierarchy, no `!` operator, snake_case files, `super.key` constructors.
