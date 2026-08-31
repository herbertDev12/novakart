import { getProducts } from "../queries/get-products";
import { ProductGrid } from "../components/list/product-grid";
import { ErrorState } from "@/components/common/error-state";

export async function FeaturedProductsContainer() {
  const result = await getProducts();
  if (!result.success) return <ErrorState error={result.error} />;

  return <ProductGrid products={result.data.items} />;
}
