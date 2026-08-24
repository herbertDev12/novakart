export const toMinorUnits = (decimal: number): number =>
  Math.round(decimal * 100);

export const fromMinorUnits = (minor: number): number => minor / 100;
