import Image from "next/image";
import { Heart, Package } from "lucide-react";
import { useTranslations } from "next-intl";
import { AspectRatio } from "@repo/components/ui/aspect-ratio";
import { Button } from "@repo/components/ui/button";
import { Card, CardContent } from "@repo/components/ui/card";
import { cn } from "@/lib/utils";
import { Money } from "@/components/common/money";
import type { MockProduct } from "@/lib/mock/home";

const tileClasses: Record<MockProduct["tile"], string> = {
  mint: "bg-tile-mint",
  blush: "bg-tile-blush",
  taupe: "bg-tile-taupe",
  sage: "bg-tile-sage",
};

type Props = {
  product: MockProduct;
  priority?: boolean;
};

export function ProductCard({ product, priority = false }: Props) {
  const t = useTranslations("common");

  return (
    <Card className="gap-0 overflow-hidden rounded-2xl py-0 transition-[transform,box-shadow] hover:-translate-y-[3px] hover:shadow-lg">
      <AspectRatio
        ratio={1}
        className={cn("overflow-hidden", tileClasses[product.tile])}
      >
        {product.imageUrl ? (
          <Image
            src={product.imageUrl}
            alt={product.name}
            fill
            sizes="(max-width: 640px) 50vw, (max-width: 880px) 33vw, 25vw"
            priority={priority}
            className="object-cover"
          />
        ) : (
          <Package
            aria-hidden
            strokeWidth={1.5}
            className="text-foreground/30 absolute inset-0 m-auto size-[34%]"
          />
        )}

        <Button
          variant="ghost"
          size="icon"
          aria-label={t("save")}
          className="bg-card/88 text-muted-foreground hover:text-pop hover:bg-card absolute top-2.5 right-2.5 size-[30px] rounded-full"
        >
          <Heart className="size-3.5" />
        </Button>
      </AspectRatio>

      <CardContent className="flex flex-col gap-1 px-3.5 py-3.5">
        <span className="text-muted-foreground text-[0.68rem] tracking-[0.06em] uppercase">
          {product.category}
        </span>
        <h3 className="text-sm leading-snug font-semibold">{product.name}</h3>
        <span className="text-primary mt-1 font-semibold tabular-nums">
          <Money value={product.price} />
        </span>
      </CardContent>
    </Card>
  );
}
