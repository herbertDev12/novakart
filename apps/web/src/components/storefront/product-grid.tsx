import { ProductCard } from "@/components/storefront/product-card";
import type { MockProduct } from "@/lib/mock/home";

type Props = {
  products: MockProduct[];
};

export function ProductGrid({ products }: Props) {
  return (
    <div className="grid grid-cols-4 gap-5 max-[880px]:grid-cols-3 max-sm:grid-cols-2">
      {products.map((product, index) => (
        <ProductCard key={product.id} product={product} priority={index < 4} />
      ))}
    </div>
  );
}
