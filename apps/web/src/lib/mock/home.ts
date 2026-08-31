// TODO(F1.x): delete this module once the home page reads promo slides from
// the real API — the product mock was replaced by
// modules/products/queries/get-products.ts in F1.2.
export type PromoSlide = {
  id: string;
  messageKey: "restock" | "shipping" | "members";
  href: string;
  tone: "sage" | "coral" | "taupe";
};

export const promoSlides: PromoSlide[] = [
  { id: "restock", messageKey: "restock", href: "#catalog", tone: "sage" },
  { id: "shipping", messageKey: "shipping", href: "#catalog", tone: "coral" },
  { id: "members", messageKey: "members", href: "#catalog", tone: "taupe" },
];
