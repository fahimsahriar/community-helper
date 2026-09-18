import { fromCsv, toCsv } from './csv';

describe('csv utils', () => {
  it('joins values with a comma and space', () => {
    expect(toCsv(['teaching', 'first-aid'])).toBe('teaching, first-aid');
    expect(toCsv([])).toBe('');
  });

  it('splits on commas, trims, and drops blanks', () => {
    expect(fromCsv('teaching, first-aid')).toEqual(['teaching', 'first-aid']);
    expect(fromCsv('  education ,, health  ,')).toEqual(['education', 'health']);
    expect(fromCsv('')).toEqual([]);
  });

  it('round-trips a list through the form representation', () => {
    expect(fromCsv(toCsv(['a', 'b']))).toEqual(['a', 'b']);
  });
});
