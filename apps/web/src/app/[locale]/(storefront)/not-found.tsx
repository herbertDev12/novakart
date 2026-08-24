import { getTranslations } from "next-intl/server";
import { buttonVariants } from "@repo/components/ui/button";
import { Link } from "@/i18n/navigation";
import { clientRoutes } from "@/routes/client-routes";

export default async function StorefrontNotFound() {
  const t = await getTranslations("errors.notFound");

  return (
    <div className="flex flex-col items-center gap-3 py-24 text-center">
      <p className="text-lg font-semibold">{t("title")}</p>
      <p className="text-muted-foreground text-sm">{t("description")}</p>
      <Link
        href={clientRoutes.storefront.home}
        className={buttonVariants({ className: "mt-2" })}
      >
        {t("cta")}
      </Link>
    </div>
  );
}
