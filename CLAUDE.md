# CommunityHelper — Claude Code Guidance

## Project Overview

CommunityHelper is a monorepo containing three interconnected applications:

- **frontend/** — Angular web application
- **backend/** — ASP.NET Core Web API (C#)
- **mobile/** — Flutter mobile application

All three apps serve the same product domain. Prefer consistency across stacks (naming, concepts, API contracts) over stack-specific cleverness.

---

## Monorepo Structure

```
CommunityHelper/
├── frontend/          # Angular 18+ web app
│   └── CLAUDE.md      # Angular-specific guidance
├── backend/           # ASP.NET Core Web API
│   └── CLAUDE.md      # Backend-specific guidance
├── mobile/            # Flutter mobile app
│   └── CLAUDE.md      # Flutter-specific guidance
├── docs/              # Architecture decisions, API contracts, shared docs
├── scripts/           # Shared build/deploy/utility scripts
├── .claude/
│   └── settings.json  # Claude Code permissions
├── .gitignore
└── CLAUDE.md          # This file
```

Each subdirectory has its own `CLAUDE.md` with stack-specific conventions. Always read the relevant `CLAUDE.md` before working in a subdirectory.

---

## How to Run Each Part

### Frontend (Angular)
```bash
cd frontend
npm install
npm start              # dev server at http://localhost:4200
npm test               # unit tests (Karma)
npx playwright test    # e2e tests
npm run build          # production build
```

### Backend (ASP.NET Core)
```bash
cd backend
dotnet restore
dotnet run --project src/API    # dev server at https://localhost:7000
dotnet test                     # all tests
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/API
```

### Mobile (Flutter)
```bash
cd mobile
flutter pub get
flutter run            # run on connected device/emulator
flutter test           # unit + widget tests
flutter build apk      # Android release build
flutter build ios      # iOS release build
```

---

## AI Usage Best Practices

### Working with Claude Code in This Project

- **Always specify the subdirectory context.** When asking for help, mention which app you are working in (frontend, backend, or mobile) and reference the relevant `CLAUDE.md`.
- **One concern per prompt.** Ask Claude to do one focused thing at a time. Avoid open-ended "build this feature end to end" prompts that span all three stacks in one shot.
- **Review before accepting.** Claude-generated code should be treated like a PR from a capable but context-limited developer. Read it, understand it, then accept it.
- **Provide file context.** When debugging, share the relevant file(s) and the exact error. Do not paraphrase errors — paste them verbatim.
- **Iterate in small steps.** Generate a skeleton, review, then flesh out. Do not generate hundreds of lines in one pass without checkpoints.
- **Do not ask Claude to manage secrets.** Never paste API keys, connection strings, or credentials into prompts.
- **Use Claude for boilerplate, not architecture decisions.** Architecture decisions should be made deliberately and documented in `docs/`. Claude can implement them, not design them.
- **Verify generated tests.** Claude-written tests can be tautological (testing mocks instead of logic). Read each test to ensure it actually validates behavior.

---

## General Best Practices

### Code Quality
- No speculative abstractions — only abstract when you have two or more concrete use cases.
- No premature optimization — write clear code first, profile before optimizing.
- Prefer explicit over clever. Code is read far more than it is written.
- Keep functions and methods small and single-purpose.
- Delete dead code immediately — do not comment it out and leave it.

### Testing
- Write tests alongside features, not after. A feature is not done until it has tests.
- Test behavior, not implementation details.
- Aim for meaningful coverage, not 100% coverage as a vanity metric.
- Every bug fix must include a regression test.

### Security
- Security is a first-class concern, not an afterthought.
- Never log sensitive data (passwords, tokens, PII).
- Validate all input on the backend, regardless of frontend validation.
- Follow OWASP Top 10 guidelines.
- Use environment variables for secrets; never hardcode them.
- Review authentication and authorization on every new endpoint/route.

### Dependencies
- Add dependencies deliberately. Evaluate maintenance status, license, and bundle/binary size impact before adding.
- Pin dependency versions in CI to prevent surprise breakage.
- Audit dependencies regularly (`npm audit`, `dotnet list package --vulnerable`, `flutter pub outdated`).

---

## Git Workflow

### Branch Naming
```
feature/<short-description>       # new functionality
fix/<short-description>           # bug fixes
chore/<short-description>         # tooling, deps, config
docs/<short-description>          # documentation only
refactor/<short-description>      # internal restructuring, no behavior change
test/<short-description>          # adding or fixing tests only
```

Examples:
- `feature/community-post-creation`
- `fix/auth-token-refresh-loop`
- `chore/update-angular-18-3`

### Commit Message Format (Conventional Commits)

```
<type>(<scope>): <short summary>

[optional body — explain WHY, not WHAT]

[optional footer: BREAKING CHANGE, closes #issue]
```

**Types:** `feat`, `fix`, `chore`, `docs`, `refactor`, `test`, `perf`, `ci`

**Scopes:** `frontend`, `backend`, `mobile`, `shared`, `infra`, `auth`, `api`

Examples:
```
feat(backend): add CQRS handler for community post creation
fix(frontend): resolve signal update loop in post-list component
chore(mobile): upgrade flutter to 3.22
test(backend): add integration tests for post creation endpoint
```

### Pull Request Checklist

Before opening a PR, confirm:

- [ ] Branch is up to date with `main`
- [ ] All tests pass locally
- [ ] No new lint warnings or errors introduced
- [ ] New functionality has corresponding tests
- [ ] No secrets, credentials, or PII committed
- [ ] `CLAUDE.md` updated if conventions changed
- [ ] `docs/` updated if architecture decisions changed
- [ ] PR description explains the "why" not just the "what"
- [ ] PR is small and focused — one concern per PR

### Branch Protection
- `main` is the stable branch. Direct pushes are not allowed.
- All changes go through PRs with at least one review.
- CI must pass before merge.

---

## Cross-Stack Conventions

### API Contract
- API contracts (request/response shapes) are the source of truth shared between backend, frontend, and mobile.
- Document all endpoints in `docs/api/`. Use OpenAPI/Swagger generated from the backend as the canonical spec.
- Frontend and mobile consume the same API — avoid building platform-specific endpoints unless strictly necessary.

### Naming Consistency
- Use the same domain terms across all three stacks. If the backend calls it `CommunityPost`, the frontend calls it `CommunityPost` (or `community-post` in kebab-case), and Flutter calls it `CommunityPost`.
- Agree on terminology before implementation and document it in `docs/domain-glossary.md`.

### Error Handling
- The backend returns structured error responses (Problem Details — RFC 7807).
- The frontend and mobile both handle these structured errors consistently.
- Never swallow errors silently in any layer.
