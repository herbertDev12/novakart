"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import { LayoutGrid, Moon, Search, ShoppingCart, Sun } from "lucide-react";
import { useTranslations } from "next-intl";
import { Avatar, AvatarFallback } from "@repo/components/ui/avatar";
import { Badge } from "@repo/components/ui/badge";
import { Button } from "@repo/components/ui/button";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@repo/components/ui/input-group";
import { useTheme } from "@repo/components/lib/theme";
import { Link } from "@/i18n/navigation";
import { clientRoutes } from "@/routes/client-routes";

export function StoreHeader() {
  const t = useTranslations("navigation");
  const tCommon = useTranslations("common");
  const { resolvedTheme, setTheme } = useTheme();

  const [mounted, setMounted] = useState(false);
  useEffect(() => setMounted(true), []);

  return (
    <header className="bg-brand text-brand-foreground shadow-sm">
      <div className="mx-auto flex max-w-[1280px] items-center gap-5 px-5 py-4">
        <Link
          href={clientRoutes.storefront.home}
          className="flex shrink-0 items-center gap-2.5 font-heading text-xl font-semibold tracking-tight"
        >
          <span className="bg-card grid size-[34px] place-items-center rounded-[10px] p-[5px]">
            <Image
              src="/novakart-logo.png"
              alt=""
              width={240}
              height={153}
              priority
              className="size-full object-contain"
            />
          </span>
          {tCommon("brand")}
        </Link>

        <Button
          variant="ghost"
          startIcon={LayoutGrid}
          className="bg-card text-foreground hover:bg-accent hover:text-accent-foreground h-[42px] shrink-0 rounded-full px-4 text-sm font-semibold max-sm:w-[42px] max-sm:px-0"
        >
          <span className="max-sm:hidden">{t("allCategories")}</span>
        </Button>

        <InputGroup className="bg-card h-[42px] max-w-[520px] flex-1 rounded-full border-transparent px-1 dark:bg-card">
          <InputGroupAddon>
            <Search className="text-muted-foreground" />
          </InputGroupAddon>
          <InputGroupInput
            type="search"
            placeholder={t("searchPlaceholder")}
            aria-label={t("searchLabel")}
            className="text-foreground h-full"
          />
        </InputGroup>

        <div className="ml-auto flex shrink-0 items-center gap-2.5">
          <Button
            variant="ghost"
            size="icon"
            aria-label={t("cartWithCount", { count: 2 })}
            className="bg-card text-foreground hover:bg-accent hover:text-accent-foreground relative size-[42px] rounded-full"
          >
            <ShoppingCart className="size-[19px]" />
            <Badge className="bg-pop text-pop-foreground border-card absolute -top-1 -right-1 size-[18px] rounded-full border-2 px-1 text-[0.65rem] font-bold">
              2
            </Badge>
          </Button>

          <Button
            variant="ghost"
            size="icon"
            aria-label={t("toggleTheme")}
            className="bg-card text-foreground hover:bg-accent hover:text-accent-foreground size-[42px] rounded-full"
            onClick={() =>
              setTheme(resolvedTheme === "dark" ? "light" : "dark")
            }
          >
            {mounted && resolvedTheme === "dark" ? (
              <Sun className="size-[18px]" />
            ) : (
              <Moon className="size-[18px]" />
            )}
          </Button>

          <Avatar className="size-[42px]">
            <AvatarFallback className="bg-card text-primary text-xs font-semibold">
              DP
            </AvatarFallback>
          </Avatar>
        </div>
      </div>
    </header>
  );
}
