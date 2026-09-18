# 01: Backend identity + profiles API

**What to build:** Volunteer and Organization admin identity on the API: register, login, token refresh, Google code exchange, plus Volunteer and Organization profile CRUD, so web and mobile have a real auth backend to build against (verifiable in Swagger).

**Blocked by:** None (can start immediately).

**Status:** done

- [x] `POST /auth/register`, `/auth/login`, `/auth/refresh`, `/auth/google` return JWT access (15 min) + rotating refresh (7 days, `refresh_tokens` collection with reuse detection)
- [x] Volunteer profile (skills, availability, causes, location, bio) and Organization profile (name, type, cause tags, location, description, `verified_at = null` = pending) CRUD work for owner only
- [x] Role claims enforce volunteer vs org_admin; unknown roles get 403; `ICurrentUserService` supplies user ID to handlers
- [x] FluentValidation on all commands via MediatR pipeline; errors return Problem Details (RFC 7807); no PII/tokens in logs
- [x] xUnit + Moq handler/validator unit tests and `WebApplicationFactory` integration tests for register/login/refresh (happy + validation-failure + unauthorized) pass
