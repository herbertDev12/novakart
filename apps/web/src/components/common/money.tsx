import type { Money } from "@/lib/types/money";
import { useFormatter } from "next-intl";

export function Money({ value }: { value: Money }) {
  const format = useFormatter();
  return (
    <span>
      {format.number(value.amountMinor / 100, {
        style: "currency",
        currency: value.currency,
      })}
    </span>
  );
}
