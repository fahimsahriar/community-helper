/**
 * Mirrors the backend `CurrentUserDto` + `AuthResultDto` returned by
 * `POST /api/auth/register`, `/login`, `/refresh`, `/google`.
 * Property names are camelCase because ASP.NET serializes DTOs that way.
 */
export const USER_ROLES = ['volunteer', 'org_admin'] as const;

export type UserRole = (typeof USER_ROLES)[number];

export function isUserRole(value: unknown): value is UserRole {
  return value === 'volunteer' || value === 'org_admin';
}

export interface CurrentUser {
  readonly id: string;
  readonly email: string;
  readonly role: UserRole;
}

export interface AuthResponse {
  readonly accessToken: string;
  readonly refreshToken: string;
  /** ISO-8601 UTC timestamp. */
  readonly expiresAtUtc: string;
  readonly user: CurrentUser;
}

export interface LoginPayload {
  readonly email: string;
  readonly password: string;
}

export interface RegisterPayload extends LoginPayload {
  readonly role: UserRole;
}
