import type { Money } from "@/lib/types/money";

export const toMinorUnits = (decimal: number): number =>
  Math.round(decimal * 100);

export const fromMinorUnits = (minor: number): number => minor / 100;

export const formatMoney = ({ amountMinor, currency }: Money, locale: string) =>
  new Intl.NumberFormat(locale, { style: "currency", currency }).format(
    fromMinorUnits(amountMinor),
  );
