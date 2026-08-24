import { getTranslations } from "next-intl/server";
import "server-only";
import { translateApiError } from "./errors";
import type { ApiError } from "@/lib/types/result";

export async function translateApiErrorServer(
  error: ApiError,
): Promise<string> {
  const t = await getTranslations();
  return translateApiError(t, error);
}
