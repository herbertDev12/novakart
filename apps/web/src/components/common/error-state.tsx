import { translateApiErrorServer } from "@/config/api/translate-api-error.server";
import type { ApiError } from "@/lib/types/result";

export async function ErrorState({ error }: { error: ApiError }) {
  const message = await translateApiErrorServer(error);

  return (
    <div className="flex flex-col items-center gap-2 py-16 text-center">
      <p className="text-sm text-muted-foreground">{message}</p>
    </div>
  );
}
