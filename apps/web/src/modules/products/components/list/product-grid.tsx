import { ProductCard } from "./product-card";
import type { Product } from "@/modules/products/types/product";

type Props = {
  products: Product[];
};

export function ProductGrid({ products }: Props) {
  return (
    <div className="grid grid-cols-4 gap-5 max-[880px]:grid-cols-3 max-sm:grid-cols-2">
      {products.map((product) => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  );
}
