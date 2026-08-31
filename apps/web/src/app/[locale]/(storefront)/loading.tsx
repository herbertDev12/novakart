import { getTranslations } from "next-intl/server";
import { buttonVariants } from "@repo/components/ui/button";
import { AspectRatio } from "@repo/components/ui/aspect-ratio";
import { Skeleton } from "@repo/components/ui/skeleton";
import { PromoCarousel } from "@/modules/promotions/components/promo-carousel";
import { SectionHeading } from "@/components/common/section-heading";
import { Link } from "@/i18n/navigation";
import { promoSlides } from "@/lib/mock/home";
import { clientRoutes } from "@/routes/client-routes";

export default async function HomeLoading() {
  const t = await getTranslations("products.catalog");
  const tCommon = await getTranslations("common");

  return (
    <div className="mx-auto max-w-[1280px] px-5 pt-6 pb-16">
      <PromoCarousel slides={promoSlides} />

      <section id="catalog">
        <SectionHeading
          title={t("title")}
          subtitle={t("subtitle")}
          action={
            <Link
              href={clientRoutes.storefront.search}
              className={buttonVariants({
                variant: "link",
                className: "h-auto px-0 font-semibold",
              })}
            >
              {tCommon("viewAll")}
            </Link>
          }
        />

        <div className="grid grid-cols-4 gap-5 max-[880px]:grid-cols-3 max-sm:grid-cols-2">
          {Array.from({ length: 8 }, (_, index) => (
            <div key={index} className="overflow-hidden rounded-2xl">
              <AspectRatio ratio={1}>
                <Skeleton className="size-full rounded-none" />
              </AspectRatio>
              <div className="flex flex-col gap-2 px-3.5 py-3.5">
                <Skeleton className="h-3 w-16" />
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-4 w-14" />
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
