import createMiddleware from "next-intl/middleware";
import { routing } from "@/i18n/routing";
import type { InterceptorFn } from "./pipeline";

export const intlInterceptor: InterceptorFn = createMiddleware(routing);
