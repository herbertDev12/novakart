"use client";

import { useEffect } from "react";
import { useTranslations } from "next-intl";
import { Button } from "@repo/components/ui/button";

export default function StorefrontError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  const t = useTranslations("errors.boundary");

  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div className="flex flex-col items-center gap-4 py-24 text-center">
      <p className="text-lg font-semibold">{t("title")}</p>
      <Button onClick={() => reset()}>{t("retry")}</Button>
    </div>
  );
}
