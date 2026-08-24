import { ArrowRight } from "lucide-react";
import { useTranslations } from "next-intl";
import {
  Carousel,
  CarouselContent,
  CarouselItem,
  CarouselNext,
  CarouselPrevious,
} from "@repo/components/ui/carousel";
import { cn } from "@/lib/utils";
import type { PromoSlide } from "@/lib/mock/home";

const toneClasses: Record<PromoSlide["tone"], string> = {
  sage: "from-tile-mint to-tile-sage before:bg-brand/45",
  coral: "from-tile-blush to-pop-soft before:bg-pop/35",
  taupe: "from-tile-taupe to-secondary before:bg-tile-sage/50",
};

type Props = {
  slides: PromoSlide[];
};

export function PromoCarousel({ slides }: Props) {
  const t = useTranslations("promotions");

  return (
    <Carousel
      className="mb-10"
      opts={{ loop: true }}
      aria-label={t("label")}
      aria-roledescription="carousel"
    >
      <CarouselContent className="ml-0">
        {slides.map((slide) => (
          <CarouselItem key={slide.id} className="pl-0">
            <div
              className={cn(
                "relative flex h-[132px] items-center gap-6 overflow-hidden rounded-[18px] bg-linear-120 shadow-sm max-sm:h-[168px]",
                // px-14 keeps the copy and CTA clear of the overlaid arrows
                "px-14 max-sm:px-12",
                "before:absolute before:-inset-[40%] before:rounded-full before:blur-3xl before:content-['']",
                toneClasses[slide.tone],
              )}
            >
              <div className="relative">
                <p className="text-foreground/65 text-[0.7rem] font-bold tracking-[0.08em] uppercase">
                  {t(`${slide.messageKey}.eyebrow`)}
                </p>
                <h2 className="font-heading mt-1 max-w-[30ch] text-[1.4rem] leading-tight font-semibold text-balance max-sm:text-[1.1rem]">
                  {t(`${slide.messageKey}.title`)}
                </h2>
              </div>

              <a
                href={slide.href}
                className="bg-card text-foreground relative ml-auto inline-flex shrink-0 items-center gap-1.5 rounded-full px-[18px] py-2.5 text-sm font-semibold shadow-sm"
              >
                <span className="max-sm:hidden">
                  {t(`${slide.messageKey}.cta`)}
                </span>
                <ArrowRight className="size-4" />
              </a>
            </div>
          </CarouselItem>
        ))}
      </CarouselContent>

      <CarouselPrevious className="bg-card/75 hover:bg-card left-3 size-8 border-none" />
      <CarouselNext className="bg-card/75 hover:bg-card right-3 size-8 border-none" />
    </Carousel>
  );
}
