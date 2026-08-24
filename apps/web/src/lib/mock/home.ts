// TODO(F1.2): delete this module once the home page reads featured products
// and promo slides from the real API.
import type { Money } from "@/lib/types/money";

export type PromoSlide = {
  id: string;
  messageKey: "restock" | "shipping" | "members";
  href: string;
  tone: "sage" | "coral" | "taupe";
};

export type MockProduct = {
  id: string;
  slug: string;
  name: string;
  category: string;
  price: Money;
  imageUrl: string | null;
  tile: "mint" | "blush" | "taupe" | "sage";
};

export const promoSlides: PromoSlide[] = [
  { id: "restock", messageKey: "restock", href: "#catalog", tone: "sage" },
  { id: "shipping", messageKey: "shipping", href: "#catalog", tone: "coral" },
  { id: "members", messageKey: "members", href: "#catalog", tone: "taupe" },
];

const usd = (amountMinor: number): Money => ({ amountMinor, currency: "USD" });

export const featuredProducts: MockProduct[] = [
  {
    id: "1",
    slug: "amber-glass-carafe",
    name: "Amber glass carafe",
    category: "Kitchen",
    price: usd(3800),
    imageUrl: "/products/amber-glass-carafe.jpg",
    tile: "mint",
  },
  {
    id: "2",
    slug: "linen-throw-pillow",
    name: "Linen throw pillow",
    category: "Textiles",
    price: usd(2900),
    imageUrl: "/products/linen-throw-pillow.jpg",
    tile: "blush",
  },
  {
    id: "3",
    slug: "terracotta-planter-small",
    name: "Terracotta planter, small",
    category: "Home",
    price: usd(2200),
    imageUrl: null,
    tile: "taupe",
  },
  {
    id: "4",
    slug: "woven-storage-basket",
    name: "Woven storage basket",
    category: "Home",
    price: usd(4600),
    imageUrl: "/products/woven-storage-basket.jpg",
    tile: "sage",
  },
  {
    id: "5",
    slug: "soy-wax-candle-cedar",
    name: "Soy wax candle, cedar",
    category: "Home",
    price: usd(2400),
    imageUrl: "/products/soy-wax-candle.jpg",
    tile: "blush",
  },
  {
    id: "6",
    slug: "stoneware-mug-set-of-2",
    name: "Stoneware mug, set of 2",
    category: "Kitchen",
    price: usd(3200),
    imageUrl: "/products/stoneware-mug.jpg",
    tile: "mint",
  },
  {
    id: "7",
    slug: "wool-throw-blanket",
    name: "Wool throw blanket",
    category: "Textiles",
    price: usd(6800),
    imageUrl: "/products/wool-throw-blanket.jpg",
    tile: "blush",
  },
  {
    id: "8",
    slug: "ceramic-serving-bowl",
    name: "Ceramic serving bowl",
    category: "Kitchen",
    price: usd(4100),
    imageUrl: "/products/ceramic-serving-bowl.jpg",
    tile: "taupe",
  },
];
