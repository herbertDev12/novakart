import type { Metadata } from "next";
import "./globals.css";
import { Figtree, Fraunces } from "next/font/google";
import { hasLocale, NextIntlClientProvider } from "next-intl";
import { setRequestLocale } from "next-intl/server";
import { notFound } from "next/navigation";
import { NuqsAdapter } from "nuqs/adapters/next/app";
import { cn } from "@/lib/utils";
import { routing } from "@/i18n/routing";
import { AuthSessionProvider } from "@/providers/session-provider";
import { ThemeProvider } from "@/providers/theme-provider";

const figtree = Figtree({ subsets: ["latin"], variable: "--font-sans" });

const fraunces = Fraunces({
  subsets: ["latin"],
  variable: "--font-heading",
  axes: ["SOFT", "opsz"],
});

export const metadata: Metadata = {
  title: "Novakart",
};

export function generateStaticParams() {
  return routing.locales.map((locale) => ({ locale }));
}

export default async function RootLayout({
  children,
  params,
}: Readonly<{
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
}>) {
  const { locale } = await params;

  if (!hasLocale(routing.locales, locale)) {
    notFound();
  }

  setRequestLocale(locale);

  return (
    <html
      lang={locale}
      className={cn("font-sans", figtree.variable, fraunces.variable)}
      suppressHydrationWarning
    >
      <body>
        <ThemeProvider>
          <NuqsAdapter>
            <NextIntlClientProvider>
              <AuthSessionProvider>{children}</AuthSessionProvider>
            </NextIntlClientProvider>
          </NuqsAdapter>
        </ThemeProvider>
      </body>
    </html>
  );
}
