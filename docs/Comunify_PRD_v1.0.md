# COMUNIFY
## Community Collaboration & Volunteer Network

> **Product Requirements Document | v1.0 | May 2025**

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Problem Statement](#2-problem-statement)
3. [Target Users & Personas](#3-target-users--personas)
4. [Product Scope — MVP vs. Future](#4-product-scope--mvp-vs-future)
5. [User Stories](#5-user-stories)
6. [Information Architecture & Key User Flows](#6-information-architecture--key-user-flows)
7. [Core Data Model](#7-core-data-model)
8. [Authentication & Authorization](#8-authentication--authorization)
9. [Non-Functional Requirements](#9-non-functional-requirements)
10. [Technology Stack](#10-technology-stack)
11. [Integration Contracts](#11-integration-contracts)
12. [Global Inclusion & Accessibility Strategy](#12-global-inclusion--accessibility-strategy)
13. [Monetization Model](#13-monetization-model)
14. [Success Metrics](#14-success-metrics)
15. [Open Questions & Risks](#15-open-questions--risks)
16. [Development Roadmap](#16-development-roadmap)

---

## 1. Executive Summary

Comunify is a community-first, open-access web platform that connects three groups who share a single goal — making the world better — but currently lack a shared, effective space to do so: individual volunteers, nonprofit and community organizations, and corporations with CSR programs.

Existing volunteer management tools are either expensive SaaS products locked behind subscription paywalls, built exclusively for large nonprofits, or isolated platforms that only serve one side of the equation. None of them adequately serve volunteers in the Global South, informal community groups, or small grassroots organizations that operate without dedicated admin staff.

Comunify closes this gap by providing a free-tier, mobile-first, multilingual platform that works equally well for a 10-person neighborhood cleanup crew in Dhaka and a 5,000-volunteer corporate social responsibility program in New York.

> **Vision Statement:** To become the world's most accessible and impactful platform where human skills, community needs, and organizational resources meet — regardless of geography, budget, or organizational size.

---

## 2. Problem Statement

### 2.1 The Core Gap

According to the 2024 Volunteer Management Progress Report, recruitment and retention have been the top challenges for volunteer managers for eight of the last nine years. Despite over 75.7 million Americans volunteering annually — and hundreds of millions more globally — matching the right person to the right need remains deeply broken.

| Who Is Affected | Pain Point |
|---|---|
| **Volunteers** | Cannot easily find opportunities that match their specific skills, schedule flexibility, or location. Sign-up processes are clunky and discouraging. |
| **Small Orgs** | Cannot afford $100–$200/month SaaS platforms. Resort to spreadsheets, WhatsApp groups, and manual email chains. |
| **Large Nonprofits** | Siloed tools that don't connect volunteer data with donor data, impact reporting, or inter-org collaboration. |
| **Developing Regions** | Almost no platforms support local languages, low-bandwidth access, or informal organization structures common in the Global South. |
| **Corporations** | CSR teams struggle to track employee volunteer hours, measure impact ROI, and find vetted local causes to support. |

### 2.2 Market Opportunity

The global Volunteer Management Platforms market was valued at $1.52 billion in 2024 and is projected to reach $3.69 billion by 2029 at an 18.8% CAGR. Yet the dominant players target enterprise nonprofits at premium price points, leaving a massive underserved segment of smaller organizations, individual volunteers, and global users.

---

## 3. Target Users & Personas

Comunify serves four primary user types, each with distinct needs and interaction patterns.

### 3.1 Persona 1 — The Skilled Individual Volunteer

> **Amara, 26, Software Developer — Lagos, Nigeria**
> Amara wants to contribute her tech skills to local causes but can't find opportunities that match her availability. Most platforms she discovers are US-centric and require lengthy sign-up forms that don't work well on her phone.

**Goals:**
- Find skill-matched, flexible (micro or project-based) volunteering opportunities
- Build a verifiable portfolio of social impact contributions
- Connect with like-minded volunteers and organizations globally

**Frustrations:**
- Complex, desktop-first onboarding flows
- No opportunities for remote or digital skills-based volunteering
- Platform language barriers (English-only interfaces)

---

### 3.2 Persona 2 — The Grassroots Organization Admin

> **Rahim, 38, NGO Coordinator — Dhaka, Bangladesh**
> Rahim manages a small flood-relief NGO with 3 staff members and 60 seasonal volunteers. He manages everything through a shared WhatsApp group and a Google Sheet. He needs help but cannot afford paid software.

**Goals:**
- Post volunteer needs and receive qualified applications quickly
- Track volunteer hours for donor reporting without manual data entry
- Connect with peer organizations for resource sharing

**Frustrations:**
- No free-tier tools with serious volunteer management features
- No way to verify and vet incoming volunteers
- Cannot measure and communicate their organization's impact to funders

---

### 3.3 Persona 3 — The Corporate CSR Manager

> **Sarah, 44, CSR Director — London, UK**
> Sarah manages employee volunteering for a 2,000-person company. Her biggest challenge is tracking hours across multiple teams, finding vetted local organizations, and producing board-level impact reports.

**Goals:**
- Organize team volunteering events with shift management
- Track and export employee volunteer hours for ESG reporting
- Demonstrate measurable community impact to leadership

**Frustrations:**
- Existing enterprise tools are expensive and poorly designed
- Hard to discover and vet legitimate local organizations to support
- No unified dashboard to see org-wide volunteering activity

---

### 3.4 Persona 4 — The Community Recipient

> **Elena, 67, Elderly Resident — Rural Romania**
> Elena's community needs help with basic tasks — transportation, food distribution, digital literacy. She represents those who benefit from volunteer work but may not even use the platform directly.

This persona represents the end beneficiary of all platform activity. Platform design decisions must always consider downstream impact on communities like Elena's.

---

## 4. Product Scope — MVP vs. Future

A disciplined MVP focuses on the core matching loop: organizations post needs, volunteers find and apply to them, participation gets tracked. Everything else is phased.

### 4.1 MVP (Version 1.0) — Core Matching Loop

| Feature | Description | Priority | Timeline |
|---|---|---|---|
| Volunteer Registration & Profile | Skills, availability, location, interests. Social login. | P0 | Week 1–4 |
| Organization Profile & Verification | Org profile, basic trust verification badge, public page. | P0 | Week 1–4 |
| Opportunity Posting | Title, skills needed, dates, location (physical/remote), capacity. | P0 | Week 1–4 |
| Browse & Search | Filter by skill, location, cause, date, remote/in-person. | P0 | Week 1–4 |
| Apply & Confirm Flow | One-click apply, org confirms/declines, notifications. | P0 | Week 5–8 |
| Volunteer Hours Tracking | Log hours per event, exportable CSV for orgs. | P1 | Week 5–8 |
| In-App Notifications | Email + push for confirmations, reminders, updates. | P1 | Week 5–8 |
| Basic Impact Dashboard | Hours contributed, number of events, volunteers helped. | P1 | Week 9–12 |
| Mobile-Responsive Web | Works on all screen sizes, PWA-ready. | P0 | Throughout |
| Multilingual Support (3 langs) | English, Spanish, Bengali as launch languages. | P1 | Week 9–12 |

### 4.2 Version 2.0 — Collaboration & Engagement

- Inter-organization resource exchange (equipment, space, expertise sharing)
- In-app messaging between volunteers and organizations
- Gamification — badges, streaks, public leaderboards
- Training & onboarding module with progress tracking
- Skills endorsement between volunteers
- Recurring events and shift scheduling
- Group/team volunteering for corporate CSR

### 4.3 Version 3.0 — Intelligence & Scale

- AI-powered smart matching (skills × availability × past performance × location)
- Corporate CSR dashboard with ESG reporting exports
- Donor-volunteer bridge (volunteers as potential donors)
- API for CRM integrations (Salesforce, Blackbaud, HubSpot)
- Background check integration for sensitive roles
- Disaster response fast-track mode
- Native mobile apps (iOS / Android)

---

## 5. User Stories

### 5.1 Volunteer Stories

| Story | Description |
|---|---|
| **Sign Up** | As a volunteer, I want to create a profile listing my skills and availability so that organizations can find me or I can be auto-matched to relevant opportunities. |
| **Discover Opportunities** | As a volunteer, I want to browse and filter opportunities by my skill set, distance, date, and cause category so that I find what suits me quickly. |
| **Apply in One Click** | As a volunteer, I want to apply to an opportunity with a single tap so that friction doesn't stop me from helping. |
| **Track My Impact** | As a volunteer, I want to see my total hours contributed and causes I've supported so that I can share my impact with employers or on my portfolio. |
| **Remote Volunteering** | As a developer/designer, I want to find short-term digital projects that use my professional skills so that I can contribute meaningfully without leaving home. |

### 5.2 Organization Stories

| Story | Description |
|---|---|
| **Post a Need** | As an org admin, I want to post a volunteer opportunity with skill requirements, time slots, and capacity limits so that I attract the right people. |
| **Vet Volunteers** | As an org admin, I want to review volunteer profiles and approve or decline applications so that I maintain quality control. |
| **Export Hours** | As an org admin, I want to export a verified log of volunteer hours and participation data so that I can include it in grant applications and donor reports. |
| **Resource Request** | As an org admin, I want to post a request for equipment or expertise to other organizations on the platform so that I can get help beyond just volunteers. |
| **View Analytics** | As an org admin, I want a dashboard showing my total volunteer hours, event completion rate, and volunteer retention so that I can measure program health. |

### 5.3 Corporate CSR Stories

| Story | Description |
|---|---|
| **Team Signup** | As a CSR manager, I want to register my company and invite employees so that all volunteering activity is tracked under one account. |
| **ESG Report** | As a CSR manager, I want to export a summary of total employee volunteer hours, causes supported, and estimated value so that I can satisfy ESG reporting requirements. |
| **Find Causes** | As a CSR manager, I want to browse verified local organizations matching our company's focus areas so that we can run meaningful team days. |

---

## 6. Information Architecture & Key User Flows

### 6.1 Core User Flows

**Volunteer Onboarding Flow**
```
Landing Page → Sign Up (social/email) → Profile Setup (skills, availability, location, causes)
→ Discover Opportunities → Apply → Confirmation → Check-in/Complete → Hours Logged → Impact Dashboard
```

**Organization Posting Flow**
```
Register Org → Verification Request → Approval (or badge pending) → Post Opportunity
→ Review Applications → Confirm Volunteers → Run Event → Mark Complete → Generate Report
```

### 6.2 Navigation Structure

| Route | Purpose |
|---|---|
| `/` | Discovery feed — nearby and recommended opportunities. Featured causes. |
| `/opportunities` | Searchable, filterable directory of all open volunteer needs. |
| `/organizations` | Verified org directory with profiles, causes, and open needs. |
| `/profile` | Volunteer dashboard: applications, hours, badges, skills. |
| `/org/dashboard` | Org admin panel: active postings, applicants, reports. |
| `/messages` | In-app messaging (v2.0). |
| `/resources` | Training materials, guides, certifications (v2.0). |
| `/admin` | Platform admin: users, orgs, flags, analytics. |

---

## 7. Core Data Model

The following entities form the foundation of the platform. Relationships define how data flows between actors.

| Entity | Key Fields | Relation | Purpose |
|---|---|---|---|
| `User` | id, name, email, role (`volunteer\|org_admin\|corp_admin\|platform_admin`), location, avatar, created_at | — | Central identity record |
| `VolunteerProfile` | user_id, skills[], causes[], availability, bio, hours_total, badges[] | User (1:1) | Extended volunteer data |
| `Organization` | id, name, type, cause_tags[], location, verified_at, admin_user_id | User (M:1) | NGO, corp, govt, informal group |
| `Opportunity` | id, org_id, title, description, skills_needed[], location, remote, capacity, start_date, end_date, status | Organization (M:1) | Volunteer need posting |
| `Application` | id, opportunity_id, volunteer_id, status (`pending\|confirmed\|declined\|completed`), applied_at | Opportunity + User | Join table with state machine |
| `VolunteerHours` | id, application_id, hours, logged_by, verified_by, notes | Application (1:1) | Audit-trail hours entry |
| `ResourceRequest` | id, org_id, type, description, status, offered_by_org_id | Organization (M:1) | Inter-org resource exchange |
| `Message` | id, sender_id, recipient_id, thread_id, content, sent_at | User (M:M) | Direct messaging |

---

## 8. Authentication & Authorization

### 8.1 Authentication Methods

- Email + password (bcrypt hashed, minimum 8 chars)
- Social login: Google, Facebook (OAuth 2.0)
- Magic link / passwordless email login for low-friction mobile access
- Optional 2FA for org admins and platform admins

### 8.2 Role Permissions Matrix

| Action | Volunteer | Org Admin | Corp Admin | Platform Admin |
|---|:---:|:---:|:---:|:---:|
| Browse opportunities | ✓ | ✓ | ✓ | ✓ |
| Apply to opportunities | ✓ | – | – | ✓ |
| Post opportunities | – | ✓ | ✓ | ✓ |
| Manage org profile | – | ✓ | ✓ | ✓ |
| View org analytics | – | ✓ | ✓ | ✓ |
| Manage team members | – | – | ✓ | ✓ |
| Export ESG reports | – | – | ✓ | ✓ |
| Verify organizations | – | – | – | ✓ |
| Manage all users | – | – | – | ✓ |

---

## 9. Non-Functional Requirements

| Requirement | Specification |
|---|---|
| **Performance** | Page load < 2.5s on 3G mobile. API response < 300ms p95. Supports 10,000 concurrent users at MVP launch. |
| **Accessibility** | WCAG 2.1 AA compliance. Screen-reader compatible. Minimum 4.5:1 color contrast ratio. Keyboard navigable throughout. |
| **Availability** | 99.5% uptime SLA. Graceful degradation — read-only browse mode if backend is unavailable. |
| **Scalability** | Horizontal scaling via containerized microservices. Database read replicas for high-traffic queries. |
| **Security** | HTTPS everywhere. OWASP Top 10 mitigations. Input sanitization. Rate limiting on all auth endpoints. |
| **Privacy & Compliance** | GDPR-compliant data handling. Right to deletion. Volunteer location data stored only with explicit consent. PDPA compliance for Bangladesh. |
| **Localization** | i18n framework from day one. RTL layout support (for Arabic/Urdu expansion). Date/time formatting per locale. |
| **Low-Bandwidth Support** | Core pages under 200KB. Progressive image loading. Works on 2G connections. Offline read mode for PWA. |

---

## 10. Technology Stack

Comunify is built on a cohesive, enterprise-grade stack centred on Angular, .NET, and Flutter — three platforms that share a strongly-typed, component-driven philosophy and have deep ecosystem support for large-scale applications.

### 10.1 Web Frontend — Angular + NgRx

| Concern | Approach |
|---|---|
| **Framework** | Angular 18 (standalone components, signals). TypeScript strict mode throughout. |
| **State Management** | NgRx (Store + Effects + Entity). Single immutable state tree for the entire web app. Selectors for derived data. Effects for all async API calls and side effects. |
| **NgRx Structure** | Feature stores per domain: `auth.store`, `opportunities.store`, `profile.store`, `org.store`, `messages.store`. Lazy-loaded feature modules map 1:1 with route-level state slices. |
| **UI Component Library** | Angular Material (CDK) as the base. Custom design tokens layered on top for Comunify brand. Storybook for component documentation. |
| **Routing** | Angular Router with lazy-loaded route modules. Route-level guards for auth and role-based access. Preloading strategy for critical paths. |
| **Forms** | Reactive Forms throughout. Custom validators for skill input, availability picker, and capacity fields. No template-driven forms. |
| **HTTP Layer** | Angular `HttpClient` with interceptors for: JWT attach, token refresh, error normalisation, and request deduplication. |
| **i18n** | `@angular/localize` + `ngx-translate` for runtime language switching without full page reload. Supports RTL layout switching. |
| **PWA** | `@angular/pwa` service worker. Background sync for offline-submitted applications. Cache strategy: network-first for API, cache-first for static assets. |
| **Testing** | Jest (unit + store logic). Cypress (E2E). Angular Testing Library for component tests. NgRx `MockStore` for isolated effect tests. |
| **Build** | Angular CLI with esbuild. Strict budget limits: initial bundle < 200KB gzipped. Per-route lazy chunks. |

### 10.2 Mobile App — Flutter + BLoC

| Concern | Approach |
|---|---|
| **Framework** | Flutter 3.x (stable channel). Dart with sound null safety. Targets Android (API 21+) and iOS (14+). |
| **State Management** | `flutter_bloc` (BLoC pattern). Every screen has a dedicated Cubit or BLoC. UI layer is purely reactive — no business logic in widgets. |
| **BLoC Structure** | One BLoC/Cubit per feature: `OpportunitiesBloc`, `ApplicationBloc`, `ProfileCubit`, `AuthBloc`, `NotificationCubit`. Events are sealed classes. States are immutable with `copyWith()`. |
| **Navigation** | GoRouter with deep link support. Route guards via redirect callbacks. Named routes for push notification deep linking. |
| **Networking** | Dio HTTP client with interceptors mirroring the web: JWT attach, refresh, retry on 401. Shared API contract with backend (OpenAPI-generated models). |
| **Offline Support** | Hive (local NoSQL) for caching opportunity feed and user profile. SQLite via `drift` for relational hour logs. Background sync on reconnect. |
| **Push Notifications** | Firebase Cloud Messaging (FCM). `flutter_local_notifications` for in-app banners. Deep link routing on tap. |
| **Maps & Location** | `flutter_map` (OpenStreetMap, no API key required for basic use) + `geolocator`. Respects Android/iOS permission model. |
| **UI / Design System** | Custom widget library matching Angular Material tokens. Shared Figma source of truth for both platforms. `flutter_screenutil` for responsive sizing. |
| **Testing** | `flutter_test` (unit + widget). Mocktail for mocking. `bloc_test` for BLoC unit tests. Integration tests with `flutter_driver` for critical flows (signup, apply, check-in). |
| **CI/CD** | GitHub Actions → build APK/IPA → deploy to Firebase App Distribution (beta) → promote to Play Store / App Store via Fastlane. |

### 10.3 Backend — ASP.NET Core

| Concern | Approach |
|---|---|
| **Framework** | .NET 8 with ASP.NET Core Web API. Minimal APIs for lightweight endpoints; Controllers for complex resource areas. |
| **Architecture** | Clean Architecture (Domain / Application / Infrastructure / API layers). CQRS pattern via MediatR — Commands for writes, Queries for reads. No business logic in controllers. |
| **Database ORM** | Entity Framework Core 8 with PostgreSQL (Npgsql provider). Migrations tracked in source control. Repository pattern over EF for testability. |
| **Auth** | ASP.NET Core Identity for user management. JWT Bearer tokens (access 15 min / refresh 7 days, rotating). OAuth 2.0 via social provider handshake delegated to frontend, tokens exchanged server-side. |
| **Real-Time** | ASP.NET Core SignalR for in-app messaging and live notifications (v2.0). Backed by Redis pub/sub for multi-instance scale-out. |
| **Background Jobs** | Hangfire with PostgreSQL storage. Recurring jobs: digest emails, expired opportunity cleanup, analytics aggregation. Fire-and-forget: post-confirmation emails, hours reminders. |
| **Caching** | `IMemoryCache` for in-process hot data. `IDistributedCache` backed by Redis for shared state across instances (search results, session data). |
| **Search** | Full-text opportunity search via PostgreSQL `tsvector` at MVP. Migrate to Elasticsearch / OpenSearch at v2.0 for faceted filtering and geospatial queries. |
| **File Storage** | Azure Blob Storage (or AWS S3) via a storage abstraction interface. Presigned URLs for direct client uploads. CDN-backed delivery. |
| **Validation** | FluentValidation for all command/query input. Problem Details (RFC 7807) for all error responses. |
| **API Documentation** | Swashbuckle (Swagger UI) + NSwag for OpenAPI spec generation. Spec used to generate TypeScript models for Angular and Dart models for Flutter. |
| **Testing** | xUnit + Moq (unit). `WebApplicationFactory` (integration). TestContainers for PostgreSQL/Redis in CI. Mutation testing via Stryker.NET on core domain logic. |

### 10.4 Shared Infrastructure

| Component | Technology |
|---|---|
| **Database** | PostgreSQL 16 (primary read-write). Read replica for analytics queries. Redis 7 (cache, pub/sub, rate limiting, distributed locks). |
| **Containerisation** | Docker for all services. Docker Compose for local development (API + DB + Redis + worker in one command). Kubernetes (AKS / EKS) for production. |
| **CI/CD** | GitHub Actions: lint → test → build → container push → deploy. Separate pipelines for web, mobile, and API. Feature branch previews via Vercel (Angular SSR) and Railway (API). |
| **Monitoring** | Application Insights (or OpenTelemetry + Grafana stack) for distributed tracing. Sentry for frontend and mobile crash reporting. Uptime Robot for availability alerts. |
| **Email** | SendGrid or AWS SES for transactional email. .NET email templates via MimeKit. Volume-based pricing — free tier covers MVP scale. |
| **Maps Tiles** | Mapbox (web + mobile). Free tier covers ~50,000 map loads/month. OpenStreetMap tile fallback for low-bandwidth regions. |
| **AI Matching (v3.0+)** | Azure OpenAI or open-source sentence-transformers (.NET ONNX Runtime). Skill embeddings stored in `pgvector` extension for cosine-similarity search. |

### 10.5 State Management Architecture — Rationale

> **NgRx (Web) vs. BLoC (Mobile)**
>
> Both patterns enforce the same principle: UI components are dumb and reactive, all business logic lives in a dedicated state layer, and data flows in a single direction. NgRx is the idiomatic choice for Angular because it integrates with Angular's dependency injection and change detection. BLoC is the Flutter community standard for the same reason. The shared .NET API and OpenAPI-generated models ensure both frontends speak the same contract — only the state container differs per platform.

---

## 11. Integration Contracts

| Integration | Specification |
|---|---|
| **Google / Facebook OAuth** | Scope: email, name, profile photo. No write permissions. Fallback: email login if blocked. |
| **Mapbox API** | Geocoding for org/volunteer locations. Opportunity radius search. Estimated monthly cost: $50–200 at MVP scale. |
| **Firebase Cloud Messaging** | Web push notifications for confirmations, reminders, messages. Requires volunteer opt-in. |
| **Email Provider (SendGrid)** | Transactional emails: welcome, confirmation, reminders, digest. Custom domain sender. |
| **CSV / Excel Export** | Server-generated via ClosedXML (.NET library). Volunteer hours and participation data exported as `.xlsx`. No client-side dependency. |
| **Google Calendar / iCal** | One-way sync: .NET generates iCal (`.ics`) feed URL per volunteer. Angular and Flutter both open the feed in the device's native calendar app. |
| **OpenAPI / NSwag** | ASP.NET Core auto-generates an OpenAPI 3.0 spec. NSwag generates TypeScript interfaces for Angular and Dart classes for Flutter — single source of truth for API contracts. |
| **SignalR (v2.0)** | ASP.NET Core SignalR hub for real-time messaging. `@microsoft/signalr` npm package for Angular. `signalr_flutter` package for Flutter. Redis backplane for multi-instance scale. |
| **Salesforce / HubSpot (v3.0)** | Webhook-based sync via .NET background worker (Hangfire). Volunteer hours and engagement data pushed on event completion. |
| **Blackbaud (v3.0)** | Donor-volunteer bridge via Blackbaud SKY API. Bidirectional sync of supporter engagement data using .NET SDK. |

---

## 12. Global Inclusion & Accessibility Strategy

Comunify is explicitly designed to be useful beyond high-income Western markets. This requires deliberate design choices from day one, not retrofitting later.

### 12.1 Language & Localization

- Launch languages: English, Spanish, Bengali
- Phase 2 additions: Arabic (RTL), French, Swahili, Hindi, Portuguese
- Community-driven translation: volunteers can contribute translations
- Locale-specific date, time, and currency formatting

### 12.2 Connectivity & Device Access

- PWA-first: installable on Android without an app store
- Core pages optimized for sub-200KB payload
- Lazy-load all images with low-quality placeholders
- Offline read-mode: cached opportunities viewable without connection
- SMS fallback for critical notifications (Twilio, opt-in only)

### 12.3 Organization Type Inclusivity

- Informal groups (neighborhood committees, religious organizations) can register without a formal legal entity
- Multi-tiered verification: Unverified → Community Verified (peer endorsed) → Officially Verified (document check)
- Free tier has no feature limits for organizations under 500 annual volunteer hours

### 12.4 Disability Accessibility

- WCAG 2.1 AA compliance as a hard launch requirement
- Screen reader compatibility tested with NVDA and VoiceOver
- No functionality gated behind color alone (colorblind-safe design)
- Minimum touch target size 44×44px on mobile

---

## 13. Monetization Model

Comunify operates on a freemium model: the core platform is permanently free for individual volunteers and small organizations. Revenue comes from premium features for large organizations and corporations.

| Tier | Price | Includes |
|---|---|---|
| **Free — Forever** | $0 | Individual volunteers (all features). Organizations up to 500 annual volunteer hours. Basic analytics. Standard listing in org directory. |
| **Pro** | $49/month | Unlimited volunteer hours. Advanced analytics and custom reports. Priority placement in search. Bulk messaging. Calendar integrations. Up to 5 staff seats. |
| **Organization+** | $149/month | Everything in Pro. Unlimited staff seats. API access. Custom branding on volunteer-facing pages. Background check integration. Dedicated support. |
| **Corporate CSR** | Custom | Team volunteering management. ESG impact reporting. Employee dashboard. Salesforce / HubSpot integration. White-label options. SLA support. |
| **Verified Badge** | $0 | Organizations earn a free verification badge by submitting documents. Builds trust without paywall. |

> **Sustainability Principle:** The free tier must always deliver core value. Paywalling core matching, posting, or hour-tracking functionality would violate the platform's mission. Monetization comes from scale features, not access.

---

## 14. Success Metrics

### 14.1 MVP Launch Targets (Month 3)

| Metric | Minimum Goal | Target Goal | Why It Matters |
|---|---|---|---|
| Registered Volunteers | 500 | 2,000 | Community seeding + organic launch |
| Registered Organizations | 50 | 150 | Direct outreach to NGOs and nonprofits |
| Opportunities Posted | 100 | 400 | Org activation rate |
| Volunteer Hours Logged | 1,000 hrs | 5,000 hrs | Core value delivery indicator |
| Volunteer-to-Opportunity Match Rate | 40% | 60% | % of postings that get confirmed volunteers |
| Mobile Traffic Share | 50% | 65% | Platform accessibility indicator |

### 14.2 Ongoing Health Metrics

- **MAV** — Monthly Active Volunteers, tracked as a monthly trend
- **Volunteer Retention Rate** — % of volunteers returning within 60 days
- **Opportunity Fill Rate** — % of posted needs that get at least one confirmed volunteer
- **Organization NPS** — quarterly survey
- **Time-to-First-Application** — from volunteer signup to first application submitted
- **HLAV** — Hours Logged per Active Volunteer, a depth-of-engagement indicator

---

## 15. Open Questions & Risks

| Risk / Question | Current Status & Decision Needed |
|---|---|
| **Volunteer Trust & Safety** | How do we protect organizations from spam applications and protect volunteers from unsafe opportunities? Decision needed: basic ID verification vs. community-based trust signals. |
| **Organization Verification Process** | Who does the verification? Automated document check, peer endorsement, or manual review? What happens to organizations that misuse the platform? |
| **Content Moderation** | Who reviews reported opportunities, messages, and profiles? Automated NLP flagging + human review queue needed. Budget allocation required. |
| **Disaster Response Mode** | A fast-track posting and volunteer mobilization flow for crisis events. Should this bypass normal verification? Detailed design TBD. |
| **Data Residency** | Where is user data stored for volunteers in the EU, Bangladesh, and other regulated markets? Multi-region deployment vs. CDN + single-region TBD. |
| **Informal Org Onboarding** | How do we verify informal community groups in low-documentation regions? Community vouching system design is needed. |
| **AI Matching Bias** | If we introduce AI-based matching, how do we ensure it doesn't systematically disadvantage volunteers in underrepresented regions or with non-English skill descriptions? |

---

## 16. Development Roadmap

| Phase | Timeline | Theme | Deliverables |
|---|---|---|---|
| **Phase 0** | Weeks 1–2 | Setup | Monorepo structure (Angular web + Flutter mobile + .NET API), CI/CD pipelines, PostgreSQL + Redis schema, design tokens in Figma, shared OpenAPI contract |
| **Phase 1** | Weeks 3–6 | Core Profiles | Angular: auth module + NgRx `auth.store`. .NET: Identity + JWT. Flutter: `AuthBloc` + onboarding screens. Volunteer & org profile CRUD. |
| **Phase 2** | Weeks 7–10 | Matching Loop | Angular: NgRx `opportunities.store` + search UI. .NET: Opportunity + Application CQRS endpoints. Flutter: `OpportunitiesBloc` + apply flow. |
| **Phase 3** | Weeks 11–14 | Analytics & Polish | NgRx Entity for collection state. .NET: Hangfire jobs for reminders + ClosedXML exports. Flutter: hours tracking Cubit. Multilingual (3 langs). |
| **Phase 4** | Weeks 15–18 | Launch Prep | Security audit, WCAG AA review, Angular PWA service worker tuning, Flutter App Distribution beta, NgRx DevTools profiling, load testing. |
| **v1.0 Launch** | Week 18–20 | Public Launch | Soft launch with 10 seed orgs. Web (Angular) and mobile (Flutter) both live. Open volunteer registration. |
| **v2.0** | Month 6–12 | Collaboration | SignalR messaging (NgRx + Flutter BLoC listeners), gamification store slice, resource exchange module, team volunteering CSR dashboard. |
| **v3.0** | Year 2 | Intelligence | AI matching via pgvector + Azure OpenAI, ESG reporting suite, native app store releases, NSwag-generated SDK for third-party integrations. |

---

*Comunify PRD v1.0 — May 2025 — Confidential*

*This document is a living specification. All sections subject to revision as research and user feedback evolves.*
