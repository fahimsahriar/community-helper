# ADR 0003 — Vitest + Playwright for web tests, not Jest + Cypress

PRD v1.0 §10.1 specifies Jest + Cypress, `frontend/CLAUDE.md` says Jasmine + Karma + Playwright, but `frontend/package.json` actually installs `vitest` + `jsdom` with no Karma/Cypress.

Decision: unit tests in Vitest, e2e in Playwright. Reason: matches installed toolchain and existing `playwright` e2e convention in repo docs; avoids adding Jest/Cypress for no coverage gain.
