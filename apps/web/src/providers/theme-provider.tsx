"use client";

import { ThemeProvider as NextThemesProvider } from "@repo/components/lib/theme";

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  return <NextThemesProvider attribute="class">{children}</NextThemesProvider>;
}
