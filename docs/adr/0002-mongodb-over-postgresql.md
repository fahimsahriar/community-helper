# ADR 0002 — MongoDB as primary store, not PostgreSQL

PRD v1.0 §10.3/10.4 specifies EF Core + PostgreSQL 16 + Redis, but `backend/` implements Clean Architecture + CQRS over `MongoDB.Driver` with `MongoDbContext`, document models, and repository interfaces (`backend/CLAUDE.md`).

Decision: stay on MongoDB for MVP. Reason: code, indexes, and plan Phase 0 already assume documents (`users.email` unique, geospatial `$nearSphere` for opportunities); migrating to Postgres now would rewrite persistence + tests with no product gain.
