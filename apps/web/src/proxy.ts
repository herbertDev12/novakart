import type { NextRequest } from "next/server";
import { InterceptorPipeline } from "./config/middleware/pipeline";
import { intlInterceptor } from "./config/middleware/intl";

// Next.js requires `config.matcher` entries to be static string literals —
// referencing a constant here fails at compile time with "Entry matcher[0]
// need to be static strings or static objects." So this pattern is written
// twice: once below as the literal Next.js requires, and once here for every
// interceptor's own matcher regex (plain runtime code, no such restriction).
// Keep both in sync.
const PUBLIC_PATHS = "api|trpc|_next|_vercel|.*\\..*";

const pipeline = new InterceptorPipeline().use(
  "intl",
  intlInterceptor,
  `/((?!${PUBLIC_PATHS}).*)`,
);

export default async function proxy(request: NextRequest) {
  return pipeline.execute(request);
}

export const config = {
  matcher: ["/((?!api|trpc|_next|_vercel|.*\\..*).*)"],
};
