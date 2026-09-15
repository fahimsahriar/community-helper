# Comunify — Development Plan
> Derived from PRD v1.0 | May 2025 | CommunityHelper Monorepo

---

## How to Read This Plan

- Each phase maps directly to the PRD roadmap (§16).
- Every task lists which layer(s) it touches: **[FE]** Angular · **[BE]** ASP.NET · **[MOB]** Flutter · **[INFRA]** Shared.
- Tasks within a phase are roughly ordered by dependency — complete top ones first.
- "Done" criteria are the acceptance conditions before moving to the next phase.

---

## Phase 0 — Project Setup (Weeks 1–2)

**Goal:** Every developer can run all three apps locally with one command. CI runs on every push.

### Infrastructure & Tooling

- [ ] **[INFRA]** Initialize git repo with branch protection on `main`
- [ ] **[INFRA]** Create `docker-compose.yml` with services: `api`, `mongodb`, `redis`, `mongo-express` (dev UI)
- [ ] **[INFRA]** Write `scripts/start-dev.sh` (or `.ps1`) — one command boots the full stack
- [ ] **[INFRA]** Set up GitHub Actions pipelines:
  - `ci-backend.yml` — restore → build → test → lint
  - `ci-frontend.yml` — npm ci → lint → test → build
  - `ci-mobile.yml` — flutter pub get → analyze → test
- [ ] **[INFRA]** Create `.env.example` with all required environment variables documented

### Backend Bootstrap

- [ ] **[BE]** `dotnet new sln` + four projects: `Domain`, `Application`, `Infrastructure`, `API`
- [ ] **[BE]** Add NuGet packages: `MediatR`, `FluentValidation`, `Serilog`, `MongoDB.Driver`, `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] **[BE]** Configure `MongoDbContext` with settings from `appsettings.json`
- [ ] **[BE]** Set up Serilog in `Program.cs` (JSON console + rolling file)
- [ ] **[BE]** Add Swashbuckle (Swagger UI) and verify it loads at `/swagger`
- [ ] **[BE]** Add global exception handling middleware (Problem Details — RFC 7807)
- [ ] **[BE]** Add MediatR pipeline behaviors: `LoggingBehaviour` → `ValidationBehaviour` → `UnhandledExceptionBehaviour`
- [ ] **[BE]** Add health check endpoint at `/health`

### Frontend Bootstrap

- [ ] **[FE]** `ng new comunify-web --standalone --routing --style=scss`
- [ ] **[FE]** Install: `@ngrx/store`, `@ngrx/effects`, `@ngrx/entity`, `@ngrx/devtools`, `@angular/material`, `@angular/pwa`, `@angular/localize`
- [ ] **[FE]** Configure Angular Material theme (M3) in `styles/_theme.scss`
- [ ] **[FE]** Set up `environment.ts` / `environment.prod.ts` with `apiUrl`
- [ ] **[FE]** Create `CoreModule`-equivalent providers: `HttpClient`, global error interceptor, JWT interceptor (stub)
- [ ] **[FE]** Verify `ng build` produces initial bundle < 200KB gzipped

### Mobile Bootstrap

- [ ] **[MOB]** `flutter create comunify_mobile --org com.comunify`
- [ ] **[MOB]** Add packages to `pubspec.yaml`: `flutter_bloc`, `go_router`, `freezed`, `json_serializable`, `dio`, `get_it`, `hive_flutter`, `flutter_local_notifications`, `firebase_messaging`
- [ ] **[MOB]** Run `dart run build_runner build` — confirm code generation works
- [ ] **[MOB]** Set up `GoRouter` with placeholder routes for `/`, `/opportunities`, `/profile`
- [ ] **[MOB]** Set up `get_it` service locator (`injection.dart`)
- [ ] **[MOB]** Configure `analysis_options.yaml` with `flutter_lints`

### Shared Contracts

- [ ] **[BE]** Add NSwag to generate OpenAPI spec on build
- [ ] **[FE]** Add NSwag npm client to generate TypeScript models from spec
- [ ] **[MOB]** Add NSwag Dart generator step — confirm generated models match backend DTOs
- [ ] **[INFRA]** Document API base URL and auth header convention in `docs/api/README.md`

### Phase 0 Done Criteria
- `docker-compose up` starts API (port 7000), MongoDB, Redis with no manual steps
- `ng serve` loads Angular app at localhost:4200 with no console errors
- `flutter run` launches mobile app on emulator with placeholder screens
- All three CI pipelines pass on a clean branch

---

## Phase 1 — Core Profiles & Authentication (Weeks 3–6)

**Goal:** A user can register, log in, and view/edit their profile on both web and mobile.

### Backend — Auth & Identity

- [ ] **[BE]** Define `User` domain entity (id, name, email, role enum, location, created_at)
- [ ] **[BE]** Define `VolunteerProfile` domain entity (skills[], causes[], availability, bio, hours_total)
- [ ] **[BE]** Define `Organization` domain entity (name, type, cause_tags[], location, verified_at)
- [ ] **[BE]** Create MongoDB document models + `BsonClassMap` registrations for all three
- [ ] **[BE]** Implement `IUserRepository`, `IVolunteerProfileRepository`, `IOrganizationRepository`
- [ ] **[BE]** Implement MongoDB indexes: `users.email` (unique), `organizations.cause_tags`, `organizations.location`
- [ ] **[BE]** Commands + handlers: `RegisterUserCommand`, `LoginCommand` (returns JWT pair)
- [ ] **[BE]** Commands + handlers: `CreateVolunteerProfileCommand`, `UpdateVolunteerProfileCommand`
- [ ] **[BE]** Commands + handlers: `RegisterOrganizationCommand`, `UpdateOrganizationCommand`
- [ ] **[BE]** Queries + handlers: `GetCurrentUserQuery`, `GetVolunteerProfileQuery`, `GetOrganizationByIdQuery`
- [ ] **[BE]** JWT access token (15 min) + refresh token (7 days, stored in MongoDB)
- [ ] **[BE]** `ICurrentUserService` — extracts user ID from `HttpContext` claims; injected into handlers
- [ ] **[BE]** Role-based authorization: `[Authorize(Roles = "volunteer")]`, `[Authorize(Roles = "org_admin")]` etc.
- [ ] **[BE]** Google OAuth 2.0 handshake endpoint (exchange code → create/find user → return JWT)
- [ ] **[BE]** FluentValidation validators for all commands above
- [ ] **[BE]** Write xUnit unit tests for all handlers and validators
- [ ] **[BE]** Write `WebApplicationFactory` integration tests for `/auth/register`, `/auth/login`, `/auth/refresh`

### Frontend — Auth Module

- [ ] **[FE]** Create `features/auth/` with routes: `/login`, `/register`, `/forgot-password`
- [ ] **[FE]** Create NgRx `auth.store`: actions (`Login`, `LoginSuccess`, `LoginFailure`, `Logout`, `RegisterUser`), reducer, effects, selectors
- [ ] **[FE]** `AuthEffect` calls `AuthService` → dispatches success/failure actions
- [ ] **[FE]** JWT interceptor: attaches `Authorization: Bearer <token>` to all API requests
- [ ] **[FE]** Token refresh interceptor: catches 401, refreshes silently, retries original request
- [ ] **[FE]** `AuthGuard` (`CanActivateFn`) — redirects unauthenticated users to `/login`
- [ ] **[FE]** `RoleGuard` — redirects users without required role to `/unauthorized`
- [ ] **[FE]** Login page (Angular Material form — email + password + Google OAuth button)
- [ ] **[FE]** Register page (role selection: volunteer vs. org admin)
- [ ] **[FE]** Volunteer profile setup flow (skills picker, availability, causes, location)
- [ ] **[FE]** Org registration form (name, type, cause tags, location, description)
- [ ] **[FE]** `profile/` route — volunteer dashboard shell with hours summary
- [ ] **[FE]** `org/dashboard` route shell — org admin panel placeholder
- [ ] **[FE]** Unit tests for `auth.store` (reducer + effects using `MockStore`)
- [ ] **[FE]** Playwright e2e: register → login → view profile flow

### Mobile — Auth BLoC & Onboarding

- [ ] **[MOB]** `AuthBloc` with events: `AuthLoginRequested`, `AuthRegisterRequested`, `AuthLogoutRequested`, `AuthTokenRefreshed`; states: `AuthInitial`, `AuthLoading`, `AuthAuthenticated`, `AuthUnauthenticated`, `AuthFailure`
- [ ] **[MOB]** `AuthRepository` interface + implementation (Dio calls to `/auth/*`)
- [ ] **[MOB]** Dio interceptor: JWT attach + silent refresh on 401
- [ ] **[MOB]** Hive storage for persisting JWT tokens across app restarts
- [ ] **[MOB]** GoRouter redirect: unauthenticated users land on `/login`
- [ ] **[MOB]** Login screen (email/password + Google OAuth button)
- [ ] **[MOB]** Register screen (role picker → volunteer or org admin)
- [ ] **[MOB]** Volunteer onboarding screens: skills, causes, availability, location
- [ ] **[MOB]** `ProfileCubit` + profile screen (view/edit volunteer profile)
- [ ] **[MOB]** `bloc_test` unit tests for `AuthBloc`
- [ ] **[MOB]** Widget tests for login and register screens

### Phase 1 Done Criteria
- User can register (volunteer or org admin) on web and mobile
- User can log in and receive a JWT; token is stored and auto-refreshed
- Volunteer can complete profile setup (skills, availability, causes)
- Org admin can create an organization profile
- All new endpoints have integration tests; all new BLoCs/NgRx stores have unit tests

---

## Phase 2 — The Matching Loop (Weeks 7–10)

**Goal:** Organizations can post opportunities; volunteers can search, find, and apply. The core value loop is complete.

### Backend — Opportunities & Applications

- [ ] **[BE]** Define `Opportunity` domain entity (org_id, title, description, skills_needed[], location, remote flag, capacity, start_date, end_date, status enum)
- [ ] **[BE]** Define `Application` domain entity (opportunity_id, volunteer_id, status state machine: pending → confirmed/declined → completed)
- [ ] **[BE]** MongoDB document models + indexes: `opportunities.skills_needed`, `opportunities.status`, `opportunities.start_date`, `opportunities.org_id`
- [ ] **[BE]** Commands + handlers: `CreateOpportunityCommand`, `UpdateOpportunityCommand`, `DeleteOpportunityCommand`
- [ ] **[BE]** Commands + handlers: `ApplyToOpportunityCommand`, `ConfirmApplicationCommand`, `DeclineApplicationCommand`, `MarkApplicationCompleteCommand`
- [ ] **[BE]** Queries + handlers: `SearchOpportunitiesQuery` (filter by skill, cause, location radius, remote, date range, pagination), `GetOpportunityByIdQuery`, `GetApplicationsByOpportunityQuery`, `GetMyApplicationsQuery`
- [ ] **[BE]** Full-text search on `title` + `description` using MongoDB Atlas Search or `$text` index
- [ ] **[BE]** Geospatial index on `opportunities.location` for radius search (`$nearSphere`)
- [ ] **[BE]** Authorization: only org admin of the posting org can confirm/decline/close
- [ ] **[BE]** FluentValidation for all commands; unit + integration tests

### Frontend — Opportunities NgRx Store & UI

- [ ] **[FE]** Create `features/opportunities/` with routes: `/opportunities`, `/opportunities/:id`, `/opportunities/create`
- [ ] **[FE]** NgRx `opportunities.store`: actions (`LoadOpportunities`, `SearchOpportunities`, `LoadOpportunityDetail`, `ApplyToOpportunity`, `CreateOpportunity`), reducer with entity adapter, effects, selectors
- [ ] **[FE]** Discovery feed page (`/`) — opportunity cards, category chips, nearby filter (uses `opportunities.store`)
- [ ] **[FE]** Search/browse page (`/opportunities`) — filter sidebar (skill, cause, date, remote toggle, location radius)
- [ ] **[FE]** Opportunity detail page — description, org info, apply button, capacity indicator
- [ ] **[FE]** One-click apply flow — confirm dialog → dispatches `ApplyToOpportunity` action
- [ ] **[FE]** Org admin: create opportunity form (multi-step: details → skills → schedule → review)
- [ ] **[FE]** Org admin: applicants list page — confirm/decline actions per applicant
- [ ] **[FE]** NgRx Entity for `opportunities` collection state (normalized, O(1) lookup by id)
- [ ] **[FE]** Unit tests: `opportunities.store` reducer + effects; Playwright e2e: search → apply flow

### Mobile — OpportunitiesBloc & Apply Flow

- [ ] **[MOB]** `OpportunitiesBloc` — events: `OpportunitiesLoadRequested`, `OpportunitiesSearchRequested`; states: `OpportunitiesLoading`, `OpportunitiesLoaded`, `OpportunitiesFailure`
- [ ] **[MOB]** `ApplicationBloc` — events: `ApplicationSubmitRequested`, `ApplicationStatusUpdateRequested`
- [ ] **[MOB]** `OpportunityRepository` + `ApplicationRepository` interfaces and implementations
- [ ] **[MOB]** Opportunity feed screen (infinite scroll list, pull-to-refresh)
- [ ] **[MOB]** Search screen with filter bottom sheet (skill chips, remote toggle, date picker)
- [ ] **[MOB]** Opportunity detail screen — apply button, org info, capacity bar
- [ ] **[MOB]** Application confirmation bottom sheet → success/failure feedback
- [ ] **[MOB]** My applications screen (tab: pending / confirmed / completed)
- [ ] **[MOB]** Hive cache for offline opportunity feed read
- [ ] **[MOB]** `bloc_test` for `OpportunitiesBloc` + `ApplicationBloc`; widget tests for feed and detail screens

### Phase 2 Done Criteria
- Org admin can post an opportunity on web and mobile
- Volunteer can search/filter opportunities and apply with one tap
- Org admin can confirm or decline applications
- Volunteer's "My Applications" tab reflects real-time status
- Geospatial and text search return relevant results

---

## Phase 3 — Analytics, Hours & Polish (Weeks 11–14)

**Goal:** Hours are tracked and exportable. The platform is multilingual. Automated reminders run.

### Backend — Hours, Jobs & Exports

- [ ] **[BE]** Define `VolunteerHours` entity (application_id, hours, logged_by, verified_by, notes)
- [ ] **[BE]** Commands + handlers: `LogVolunteerHoursCommand`, `VerifyVolunteerHoursCommand`
- [ ] **[BE]** Query: `GetOrganizationHoursReportQuery` (aggregated hours by event, by volunteer, by date range)
- [ ] **[BE]** Export endpoint: `GET /org/{id}/reports/hours` → returns `.xlsx` via ClosedXML
- [ ] **[BE]** iCal feed endpoint: `GET /volunteers/{id}/calendar.ics`
- [ ] **[BE]** Add Hangfire: configure PostgreSQL (or MongoDB) storage, dashboard at `/hangfire` (admin-only)
- [ ] **[BE]** Hangfire recurring job: `ExpiredOpportunityCleanupJob` (daily)
- [ ] **[BE]** Hangfire fire-and-forget: confirmation email on application confirmed
- [ ] **[BE]** Hangfire fire-and-forget: reminder email 24h before event
- [ ] **[BE]** Set up SendGrid client (or AWS SES) with email template for confirmations and reminders
- [ ] **[BE]** Aggregate query: `GetImpactDashboardQuery` (total hours, events, volunteers for a volunteer or org)

### Frontend — Impact Dashboard & i18n

- [ ] **[FE]** NgRx `profile.store` extension: add `hoursLog`, `impactSummary` slices
- [ ] **[FE]** Volunteer impact dashboard (`/profile/impact`) — total hours chart, causes breakdown, badges placeholder
- [ ] **[FE]** Org analytics dashboard (`/org/dashboard/analytics`) — hours per event table, volunteer retention card, export button
- [ ] **[FE]** Export button → calls hours report endpoint → triggers browser download of `.xlsx`
- [ ] **[FE]** "Log hours" flow for org admin (post-event, per volunteer)
- [ ] **[FE]** Set up `@angular/localize` + `ngx-translate`: extract translation keys, create `en.json`, `es.json`, `bn.json`
- [ ] **[FE]** Language switcher component in app header (persists choice in localStorage)
- [ ] **[FE]** RTL layout toggle foundation (for future Arabic support)
- [ ] **[FE]** PWA: configure `ngsw-config.json` — network-first for API, cache-first for assets; test offline browse mode

### Mobile — Hours Cubit & Notifications

- [ ] **[MOB]** `HoursCubit` — states for logging and viewing hours per application
- [ ] **[MOB]** Hours logging screen (post-event, accessible from My Applications)
- [ ] **[MOB]** Volunteer impact screen — total hours, causes, contribution history
- [ ] **[MOB]** Configure Firebase Cloud Messaging: request permission, store FCM token on backend
- [ ] **[MOB]** `NotificationCubit` — handle incoming FCM messages, route deep links via GoRouter
- [ ] **[MOB]** `flutter_local_notifications` — in-app banners for confirmations and reminders
- [ ] **[MOB]** Set up `intl` package for localization; add English, Spanish, Bengali ARB files
- [ ] **[MOB]** Hive offline cache for hours log (sync on reconnect)
- [ ] **[MOB]** `bloc_test` for `HoursCubit`; widget tests for hours screen

### Phase 3 Done Criteria
- Org admin can export verified volunteer hours as `.xlsx`
- Volunteer sees total hours and impact summary on their dashboard
- Platform displays correctly in English, Spanish, and Bengali
- Automated confirmation and reminder emails are delivered
- PWA installs on Android and works in offline read mode

---

## Phase 4 — Launch Preparation (Weeks 15–18)

**Goal:** The platform is production-hardened, accessible, and load-tested.

### Security

- [ ] **[BE]** Rate limiting on all `/auth/*` endpoints (e.g. 10 req/min per IP via `AspNetCoreRateLimit`)
- [ ] **[BE]** Input sanitization review — verify all user-generated content is sanitized before storage
- [ ] **[BE]** Security headers middleware: `X-Content-Type-Options`, `X-Frame-Options`, `Content-Security-Policy`
- [ ] **[BE]** Review all endpoints against role permissions matrix (PRD §8.2) — no privilege escalation paths
- [ ] **[BE]** Verify JWT validation: `aud`, `iss`, `exp` all enforced
- [ ] **[BE]** Run OWASP ZAP scan against staging API; remediate all high/medium findings
- [ ] **[FE]** CSP policy configured in Angular; no inline scripts
- [ ] **[FE]** XSS review: all user content rendered with Angular's built-in sanitization (no `innerHTML` bypasses)
- [ ] **[MOB]** Certificate pinning for production API (Android + iOS)
- [ ] **[MOB]** No secrets in `pubspec.yaml` or Dart source; use `--dart-define` for env vars

### Accessibility (WCAG 2.1 AA)

- [ ] **[FE]** Run `axe-core` Playwright integration — fix all critical + serious violations
- [ ] **[FE]** Manual keyboard navigation audit: every interactive element reachable and operable by keyboard
- [ ] **[FE]** Color contrast audit: all text meets 4.5:1 ratio (use `ng lint` + manual review)
- [ ] **[FE]** Screen reader test with NVDA (Windows) and VoiceOver (Mac) on core flows
- [ ] **[MOB]** Verify minimum touch targets 44×44px throughout
- [ ] **[MOB]** `Semantics` widget review — all meaningful widgets have accessible labels

### Performance

- [ ] **[BE]** Add MongoDB query explain plan review for all frequent queries; add missing indexes
- [ ] **[BE]** Redis caching for `SearchOpportunities` results (TTL: 60s)
- [ ] **[BE]** Load test with k6: 10,000 concurrent users, p95 API response < 300ms
- [ ] **[FE]** `ng build --configuration=production` — verify initial bundle < 200KB gzipped
- [ ] **[FE]** Lighthouse CI in GitHub Actions — enforce Performance ≥ 85, Accessibility = 100
- [ ] **[MOB]** Profile with Flutter DevTools — no jank on opportunity feed scroll (60fps)
- [ ] **[MOB]** Verify core screens < 200KB payload on simulated 3G

### Release Preparation

- [ ] **[INFRA]** Docker images built and pushed to container registry (GitHub Container Registry or ACR)
- [ ] **[INFRA]** Kubernetes manifests (or Railway / Render config) for API + Redis
- [ ] **[INFRA]** Set up Application Insights (or OpenTelemetry + Grafana) — traces flowing
- [ ] **[INFRA]** Set up Sentry for Angular (DSN in `environment.prod.ts`) and Flutter (`sentry_flutter`)
- [ ] **[INFRA]** Uptime monitoring configured (Uptime Robot or Better Uptime)
- [ ] **[FE]** Deploy Angular app to Vercel (or Azure Static Web Apps) with production environment variables
- [ ] **[MOB]** Upload signed APK to Firebase App Distribution for beta testers
- [ ] **[MOB]** TestFlight build submitted for iOS beta review

### Phase 4 Done Criteria
- OWASP ZAP scan: zero high/critical findings
- Lighthouse CI: Accessibility score = 100 on all main routes
- k6 load test passes at 10,000 concurrent users
- Staging environment fully deployed and stable for 48 hours
- Beta APK distributed to ≥ 10 real-device testers with no crash reports

---

## v1.0 Public Launch (Weeks 18–20)

- [ ] Soft launch with 10 seed organizations onboarded manually
- [ ] Open volunteer registration (remove invite gate if any)
- [ ] Monitor Sentry for crash spikes in first 48 hours
- [ ] Monitor MongoDB slow query log daily for first week
- [ ] Weekly active user dashboard live in Application Insights
- [ ] Hotfix process documented: `fix/*` branch → PR → merge → auto-deploy < 30 min

---

## v2.0 — Collaboration & Engagement (Months 6–12)

> Reference: PRD §4.2

- [ ] **[BE]** SignalR hub for real-time messaging; Redis backplane for multi-instance
- [ ] **[FE]** NgRx `messages.store`; `@microsoft/signalr` client in Angular
- [ ] **[MOB]** `MessagingBloc`; `signalr_flutter` client
- [ ] **[BE/FE/MOB]** Gamification — badges and streaks (NgRx slice + Flutter Cubit)
- [ ] **[BE/FE/MOB]** Resource exchange module (`ResourceRequest` entity, CQRS, UI)
- [ ] **[FE/MOB]** Group/team volunteering for corporate CSR
- [ ] **[BE]** Recurring events and shift scheduling
- [ ] **[FE]** Storybook component documentation

---

## v3.0 — Intelligence & Scale (Year 2)

> Reference: PRD §4.3

- [ ] **[BE]** AI matching: skill embeddings via MongoDB Atlas Vector Search or pgvector
- [ ] **[BE]** Corporate CSR dashboard + ESG reporting exports
- [ ] **[BE]** Salesforce / HubSpot webhook sync (Hangfire background worker)
- [ ] **[BE]** Blackbaud SKY API donor-volunteer bridge
- [ ] **[MOB]** Submit to Google Play Store and Apple App Store (native app store releases)
- [ ] **[BE]** OpenAPI SDK published for third-party integrations
- [ ] **[INFRA]** Multi-region deployment for EU/BD data residency (addresses PRD §15 open risk)

---

## Cross-Cutting Concerns (Every Phase)

These apply throughout all phases — not one-off tasks.

| Concern | Rule |
|---|---|
| **API contract** | Never break the OpenAPI spec without a version bump. Regenerate NSwag clients after every schema change. |
| **MongoDB indexes** | Review `explain()` output for every new query before merging. |
| **Tests** | No feature is merged without tests. Unit tests for handlers/BLoCs/NgRx effects. Integration tests for new endpoints. |
| **Security** | Every new endpoint reviewed against the role permissions matrix (PRD §8.2) before merge. |
| **Accessibility** | Run `axe-core` on every new Angular page before merge. Check touch targets on every new Flutter screen. |
| **i18n** | Every user-visible string goes through the translation pipeline from day one — no hardcoded English strings in templates or widgets. |
| **No PII in logs** | Serilog and Flutter logging must never emit email, name, location, or token values. |
| **PR size** | One concern per PR. If a PR touches all three stacks, split it. |

---

*plan.md — Comunify — Derived from PRD v1.0 — May 2025*
