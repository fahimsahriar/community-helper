/** Shared comma-separated-value helpers for profile form fields. */

/** Renders a string list for editing, e.g. `['a', 'b']` → `'a, b'`. */
export function toCsv(values: readonly string[]): string {
  return values.join(', ');
}

/** Parses a comma-separated input, dropping blanks, e.g. `'a, , b'` → `['a', 'b']`. */
export function fromCsv(value: string): string[] {
  return value
    .split(',')
    .map((part) => part.trim())
    .filter((part) => part.length > 0);
}
