/**
 * Mirrors `VolunteerProfileDto` returned by
 * `GET /api/volunteer-profiles/me` (camelCase via ASP.NET JSON).
 */
export interface VolunteerProfile {
  readonly userId: string;
  readonly skills: readonly string[];
  readonly availability: string;
  readonly causes: readonly string[];
  readonly location: string;
  readonly bio: string;
}

export interface VolunteerProfilePayload {
  readonly skills: readonly string[];
  readonly availability: string;
  readonly causes: readonly string[];
  readonly location: string;
  readonly bio: string;
}
