/**
 * Mirrors `OrganizationDto` returned by `/api/organizations/*`
 * (camelCase via ASP.NET JSON). `verifiedAtUtc === null` means the
 * organization is pending verification (Phase 1 shows a badge only).
 */
export interface Organization {
  readonly id: string;
  readonly ownerUserId: string;
  readonly name: string;
  readonly type: string;
  readonly causeTags: readonly string[];
  readonly location: string;
  readonly description: string;
  /** ISO-8601 UTC timestamp, or null when pending verification. */
  readonly verifiedAtUtc: string | null;
  readonly isVerified: boolean;
}

export interface OrganizationPayload {
  readonly name: string;
  readonly type: string;
  readonly causeTags: readonly string[];
  readonly location: string;
  readonly description: string;
}
