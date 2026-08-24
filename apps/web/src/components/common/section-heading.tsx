import type { ReactNode } from "react";

type Props = {
  title: string;
  subtitle?: string;
  action?: ReactNode;
};

export function SectionHeading({ title, subtitle, action }: Props) {
  return (
    <div className="mb-5 flex items-baseline justify-between gap-4">
      <div>
        <h2 className="font-heading text-2xl font-semibold text-balance">
          {title}
        </h2>
        {subtitle && (
          <p className="text-muted-foreground mt-1 text-sm">{subtitle}</p>
        )}
      </div>
      {action}
    </div>
  );
}
