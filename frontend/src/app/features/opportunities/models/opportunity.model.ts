/** Mirrors `OpportunityDto` returned by GET /api/opportunities. */
export interface Opportunity {
  readonly id: string;
  readonly title: string;
  readonly description: string;
  readonly location: string;
  readonly isRemote: boolean;
  /** ISO-8601 UTC timestamp. */
  readonly startsAtUtc: string;
}
