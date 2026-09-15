/**
 * RFC 7807 Problem Details — the structured error shape returned by the
 * CommunityHelper API. Kept in `core` because every feature consumes it.
 */
export interface ProblemDetails {
  readonly type?: string;
  readonly title?: string;
  readonly status?: number;
  readonly detail?: string;
  readonly instance?: string;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
}
