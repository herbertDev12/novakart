"use client";

import { useTranslations } from "next-intl";
import { toast } from "@repo/components/lib/toast";
import { translateApiError } from "@/config/api/errors";
import type { ApiError } from "@/lib/types/result";

export function useApiErrorToast() {
  const t = useTranslations();

  return (error: ApiError) => {
    toast.error(translateApiError(t, error));
  };
}
