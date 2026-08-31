import "server-only";

import { ok, type Result } from "@/lib/types/result";
import type { Paginated } from "@/lib/types/pagination";
import type { Product } from "../types/product";

// TODO(F1.x): swap this mock for a real fetcher(apiRoutes.products.list,
// { next: { tags: [CacheTags.products.list] } }) call once the endpoint exists.
export async function getProducts(): Promise<Result<Paginated<Product>>> {
  const items: Product[] = [
    {
      id: "1",
      name: "Amber glass carafe",
      description: "Hand-blown amber glass carafe for water or wine.",
      price: { amountMinor: 3800, currency: "USD" },
      stock: 12,
      isActive: true,
      category: { id: "cat-1", name: "Kitchen" },
    },
    {
      id: "2",
      name: "Linen throw pillow",
      description: "Soft linen throw pillow, natural dye.",
      price: { amountMinor: 2900, currency: "USD" },
      stock: 30,
      isActive: true,
      category: { id: "cat-2", name: "Textiles" },
    },
  ];

  return ok({
    items,
    page: 1,
    perPage: items.length,
    total: items.length,
    totalPages: 1,
  });
}
