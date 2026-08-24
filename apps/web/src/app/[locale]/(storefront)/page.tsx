import { getTranslations, setRequestLocale } from "next-intl/server";
import { buttonVariants } from "@repo/components/ui/button";
import { PromoCarousel } from "@/modules/promotions/components/promo-carousel";
import { ProductGrid } from "@/modules/products/components/list/product-grid";
import { SectionHeading } from "@/components/common/section-heading";
import { Link } from "@/i18n/navigation";
import { featuredProducts, promoSlides } from "@/lib/mock/home";
import { clientRoutes } from "@/routes/client-routes";

type Props = {
  params: Promise<{ locale: string }>;
};

export default async function Home({ params }: Props) {
  const { locale } = await params;
  setRequestLocale(locale);

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
        <ProductGrid products={featuredProducts} />
      </section>
    </div>
  );
}
