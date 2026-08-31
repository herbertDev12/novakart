import type { Money } from "@/lib/types/money";

export type ProductCategory = {
  id: string;
  name: string;
};

export type Product = {
  id: string;
  name: string;
  description: string;
  price: Money;
  stock: number;
  isActive: boolean;
  category: ProductCategory;
};

export type ProductSummary = {
  id: string;
  name: string;
  price: Money;
  isActive: boolean;
  categoryId: string;
};
