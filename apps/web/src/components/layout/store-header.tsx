import { Link } from "@/i18n/navigation";

export function StoreHeader() {
  return (
    <header className="border-b">
      <div className="mx-auto flex h-16 max-w-6xl items-center px-4">
        <Link href="/" className="text-lg font-semibold tracking-tight">
          Novakart
        </Link>
      </div>
    </header>
  );
}
