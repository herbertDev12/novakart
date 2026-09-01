# FEATURE_ROADMAP.md

Vertical delivery plan for the Novakart storefront. **Read [`WEB_ARCHITECTURE.md`](./WEB_ARCHITECTURE.md) first** — it is normative and this document never overrides it.

| Document | Answers | Changes when |
|---|---|---|
| `WEB_ARCHITECTURE.md` | **How** we build: layers, conventions, rules | A convention is proven wrong |
| `FEATURE_ROADMAP.md` (this) | **What** we build, in what order, and where it stands | A slice is delivered, or scope changes |

---

## Table of contents

1. [Why this document exists](#1-why-this-document-exists)
2. [What a vertical slice is — and is not](#2-what-a-vertical-slice-is--and-is-not)
3. [The slice contract](#3-the-slice-contract)
4. [How to read a feature entry](#4-how-to-read-a-feature-entry)
5. [Current state — honest inventory](#5-current-state--honest-inventory)
6. [Phase 0 — Realignment and foundation debt](#phase-0--realignment-and-foundation-debt)
7. [Phase 1 — Catalog](#phase-1--catalog)
8. [Phase 2 — Cart](#phase-2--cart)
9. [Phase 3 — Identity](#phase-3--identity)
10. [Phase 4 — Checkout and orders](#phase-4--checkout-and-orders)
11. [Phase 5 — Customer account](#phase-5--customer-account)
12. [Phase 6 — Back office](#phase-6--back-office)
13. [Phase 7 — Production readiness](#phase-7--production-readiness)
14. [Dependency map](#14-dependency-map)
15. [Conventions this document adds](#15-conventions-this-document-adds)

---

## 1. Why this document exists

The first execution plan was organized **by layer**. The commit log shows the result:

```
feat(web): add shared data contracts for the data access layer
feat(web): add route and cache-tag catalogs
feat(web): add fetcher retry policy and error translation
feat(web): add the http fetcher
feat(web): add error boundaries and shared empty/error/header states
feat(web): add real translation namespaces and a message-drift check
feat(web): add the storefront shell (header, footer, layout)
```

Seven commits of infrastructure. At the end of them:

- `httpFetcher` existed and **nothing called it**.
- `apiRoutes` existed and **nothing requested those URLs**.
- `CacheTags` existed and **nothing invalidated a tag**.
- `src/` had **no `modules/` directory at all**.
- The home page was `<h1>Novakart</h1>`.

Every layer had a foundation. No layer had a consumer. That is the failure mode this document exists to prevent: **code that accumulates without integrating**.

The architecture was never the problem — it is good and it stays. The *sequencing* was the problem.

---

## 2. What a vertical slice is — and is not

A vertical slice is **one user-visible capability, working end to end, through every layer it touches**.

> **A slice is not permission to skip layers.**
> It is permission to build only the *part of each layer* that this capability needs.

Concretely, "list products in a category" is one slice. Delivering it means:

```
routes/api-routes.ts        + categories.bySlug, products.byCategory   ← only these keys
routes/cache-tags.ts        + the tags those queries need              ← only these
modules/products/types/     + Product                                  ← only this entity
modules/products/queries/   + getProductsByCategory                    ← only this query
modules/products/containers + product-list-container                   ← only this container
modules/products/components + the list components                      ← only these
app/…/c/[slug]/page.tsx     + page, loading, error, metadata
messages/{en,es}.json       + the products namespace keys this uses
```

It touches **eight layers** — exactly as [§6](./WEB_ARCHITECTURE.md#6-the-request-lifecycle) prescribes — but adds only what this one capability consumes. Nothing speculative. Nothing unconsumed.

**The test:** after the slice merges, can a person open a URL and do the thing? If not, it was not a slice.

### What changes, and what does not

| Unchanged | Changed |
|---|---|
| The layer order in [§6](./WEB_ARCHITECTURE.md#6-the-request-lifecycle) | When each layer gets built |
| The 10 golden rules in [§1](./WEB_ARCHITECTURE.md#1-golden-rules) | Nothing |
| The directory contract in [§5](./WEB_ARCHITECTURE.md#5-directory-structure) / [§22](./WEB_ARCHITECTURE.md#22-naming-and-code-conventions) | Nothing |
| The scaffold recipe in [§24](./WEB_ARCHITECTURE.md#24-recipe-scaffold-a-new-module) | Applied per-slice instead of all at once |
| The definition of done in [§25](./WEB_ARCHITECTURE.md#25-definition-of-done) | Nothing — it now gates each slice |

The recipe in §24 still runs top to bottom. It just runs **narrow**: for one capability, not for a whole domain up front.

---

## 3. The slice contract

**No slice merges until every applicable line is true.** This is [§25](./WEB_ARCHITECTURE.md#25-definition-of-done) restated as a per-slice gate, plus the two lines that specifically prevent the horizontal failure.

**Integration — the two that are new**
- [ ] Every file added is reachable from a route a user can open. No orphan modules.
- [ ] Every catalog key added (`api-routes`, `cache-tags`, `client-routes`) has a caller **in this same slice**.

**Correctness**
- [ ] `pnpm -r check-types` and `pnpm -r lint` pass with zero warnings.
- [ ] `node apps/web/scripts/check-messages.mjs` passes.
- [ ] Loading, empty, and error states are implemented — not just the happy path.
- [ ] New logic touching money, cart, or checkout has tests.

**Architecture**
- [ ] Client boundary is as deep as possible; no unnecessary `"use client"`.
- [ ] Every new Server Action validates input **and** checks authorization.
- [ ] Every mutation invalidates the right cache tags.
- [ ] List state is in the URL with the standard parameter names.
- [ ] No second implementation of a mechanism that already exists.
- [ ] Folder and file naming follow [§22](./WEB_ARCHITECTURE.md#22-naming-and-code-conventions) exactly.

**Commerce**
- [ ] All amounts are integer minor units with an explicit currency.
- [ ] No price, discount, or stock value is trusted from the client.
- [ ] Personalized routes are `force-dynamic` and not cached.
- [ ] Money-affecting operations are idempotent where retries are possible.

**Front of house**
- [ ] Public routes export real `generateMetadata`.
- [ ] Images have explicit dimensions and meaningful `alt`.
- [ ] All user-facing strings come from message files, in **all** locales.

**Verification** — for any slice with a visual surface, take a screenshot of the rendered route and look at it. Passing type-check, lint and build has already been demonstrated to be compatible with a completely broken page.

---

## 4. How to read a feature entry

```
### F1.3 — Category listing page                                   ⬜
Goal        What a user can do when this is done.
Depends     F1.1
Module      modules/products
Route       /[locale]/c/[slug]
Layers      routes → types → queries → container → components → page → i18n
Arch        §7, §11.4, §16.3, §13 Tier 1
Done when   The acceptance statement. Observable, not "code written".
```

**Status legend**

| | Meaning |
|---|---|
| ✅ | Done — meets the slice contract |
| 🟡 | Partial — exists but does not meet the contract (the gap is named) |
| ⬜ | Not started |
| 🔒 | Blocked by an unfinished dependency |

**Phases are ordered by dependency, not by importance.** Inside a phase, slices can often go in parallel.

---

## 5. Current state — honest inventory

### Built and meeting the contract

| Area | Detail |
|---|---|
| Data contracts | `Result<T>`, `Paginated<T>`, `SearchParams`, `Money` |
| Retry policy | `RETRY`, disjoint `TERMINAL_STATUSES` / `RETRYABLE_STATUSES` |
| i18n | `routing.ts`, `navigation.ts`, `request.ts`, locale cookie, message-drift check in CI |
| Global boundaries | `app/global-error.tsx`, `app/global-not-found.tsx` |
| Shared states | `components/common/{empty-state,error-state,page-header}.tsx` |
| Design system | `@repo/components` with ~43 shadcn primitives, `transpilePackages`, `@source` wiring |
| Design tokens | Novakart palette mapped onto shadcn semantic tokens, light + dark |
| Storefront chrome | Header (topbar, search, cart, account) and footer, presentational |
| Home page | Carousel + product grid, rendering, screenshot-verified in both themes |

### Built but incomplete against the architecture

| Area | Gap | Ref |
|---|---|---|
| `config/api/http-fetcher.ts` | Does not inject `Authorization`; no `skipAuth` option | §11.2 |
| `config/api/errors.ts` | `translateApiError` is missing | §11.7 |
| `proxy.ts` | Bare `next-intl` middleware, not the composable pipeline; no auth interceptor | §9 |
| `auth.ts` | `providers: []`; no session type augmentation, no refresh | §10 |
| `routes/api-routes.ts` | Only `products` and `categories`; no `bySlug` | §11.3 |
| `routes/cache-tags.ts` | Only `products` and `categories` | §11.3 |
| `routes/client-routes.ts` | Storefront only; no admin, no auth routes | §11.3 |
| `lib/utils/money.ts` | Hand-rolled `Intl.NumberFormat`; §8 and §16.1 require the `useFormatter`-based `<Money>` component | §8, §16.1 |

### Built outside the architecture — introduced during the home page work

| What | Where it is | Where it belongs | Ref |
|---|---|---|---|
| `product-card.tsx`, `product-grid.tsx` | `components/storefront/` | `modules/products/components/` | §5, §15 |
| `promo-carousel.tsx` | `components/storefront/` | `modules/promotions/components/` | §5 |
| `section-heading.tsx` | `components/storefront/` | `components/common/` | §5 |
| `home.ts` mock data | `lib/mock/` | Deleted when F1.2 lands | §5 |
| `home` message namespace | `messages/*.json` | Namespaces match module names | §8 |

`components/storefront/` and `lib/mock/` are **not folders the architecture defines**. Fixing this is [F0.1](#f01--realign-the-home-page-into-the-architecture-), and it is the first slice.

### Not started

`modules/` (nothing), `config/auth/`, `config/middleware/`, `lib/auth/`, `lib/schemas/`, `hooks/`, `components/data-table/`, `routes/navigation-data.ts`, tests, Sentry, `sitemap.ts`, `robots.ts`, `.env.example`.

---

## Phase 0 — Realignment and foundation debt

**Goal:** every later slice can be built without inventing structure or working around a known gap.

These are not features. They are the price of the horizontal phase, paid once. Keep this phase short — anything not blocking a Phase 1 slice belongs in a later phase.

---

### F0.1 — Realign the home page into the architecture ✅

| | |
|---|---|
| **Goal** | The home page renders identically, from paths the architecture defines. |
| **Depends** | — |
| **Layers** | modules → components → i18n |
| **Arch** | §5, §15, §22, §8 |

Steps:

1. Create `modules/products/` and `modules/promotions/` with the [§5](./WEB_ARCHITECTURE.md#5-directory-structure) anatomy — **only the folders that get content**. No `.gitkeep` scaffolding.
2. Move `product-card.tsx` and `product-grid.tsx` → `modules/products/components/list/`.
3. Move `promo-carousel.tsx` → `modules/promotions/components/`.
4. Move `section-heading.tsx` → `components/common/` (it is domain-free; §15 says app-wide shared).
5. Delete `components/storefront/`.
6. Rename the `home` message namespace. Split its keys into `products` and `promotions` to match the module names ([§8](./WEB_ARCHITECTURE.md#8-internationalization)).
7. Leave `lib/mock/home.ts` in place for now — F1.2 deletes it. Add a `TODO(F1.2)` at the top.

**Done when** `/en` and `/es` render exactly as before, `components/storefront/` no longer exists, and every namespace name matches a module name.

> Screenshot before and after and compare. This is a pure move; any visual difference is a mistake.

---

### F0.2 — The money render boundary ✅

| | |
|---|---|
| **Goal** | Every price in the app renders through one locale-aware component. |
| **Depends** | — |
| **Arch** | §8, §16.1, §22 |

- Add `<Money value={money} />` to `components/common/money.tsx`, built on `useFormatter()` from next-intl.
- Replace `formatMoney` usage in the product card.
- Keep `toMinorUnits` / `fromMinorUnits` in `lib/utils/money.ts` — those are pure arithmetic and belong there.
- Delete `formatMoney`: §8 says *never hand-roll*, and two formatters is anti-pattern #6.

**Done when** no component calls `Intl.NumberFormat` directly, and prices render correctly in both locales (`$38.00` / `38,00 US$`).

---

### F0.3 — API error translation ✅

| | |
|---|---|
| **Goal** | A backend error code becomes a translated message, once, in one place. |
| **Depends** | — |
| **Arch** | §11.7 |

- Implement `translateApiError(t, error)` in `config/api/errors.ts`.
- Add **exactly two** thin adapters: one for Server Components (`getTranslations`), one for Client Components (`useTranslations` + toast).
- Wire `components/common/error-state.tsx` to use the server adapter.

**Done when** `<ErrorState>` shows a translated message for a known code and the raw message for an unknown one, in both locales.

---

### F0.4 — Auth-aware fetcher ✅

| | |
|---|---|
| **Goal** | Server-side requests carry the session token without any call site thinking about it. |
| **Depends** | — |
| **Arch** | §11.2 |

- Add the `Authorization: Bearer` header from the session, and a `skipAuth` option for public catalog reads.
- Keep `import "server-only"`.
- Rename to `fetcher.ts` to match §11.2, or record the deviation. Do not keep two fetchers.

**Done when** an authenticated read reaches the API with the header, and a `skipAuth` read does not.

> Depends on the session shape from [F3.1](#f31--auth-providers-session-and-refresh-). Build the `skipAuth` path now — Phase 1 is entirely public reads — and add the token injection in F3.1. Note it in the code as `TODO(F3.1)`.

---

### F0.5 — Interceptor pipeline ✅

| | |
|---|---|
| **Goal** | `proxy.ts` composes independent concerns instead of being one library's middleware. |
| **Depends** | — |
| **Arch** | §9 |

- Add `config/middleware/pipeline.ts` with `InterceptorPipeline`.
- Move the current next-intl middleware into `config/middleware/intl.ts`.
- Define `PUBLIC_PATHS` **once** and reuse it in every matcher.
- Merge headers and cookies forward between interceptors.
- Leave the auth interceptor slot empty until F3.3.

**Done when** locale routing behaves exactly as today, through the pipeline, and the exclusion pattern is written once.

---

### F0.6 — Route-group error boundaries ✅

| | |
|---|---|
| **Goal** | A failure inside the storefront shows a recoverable UI, not a blank page. |
| **Depends** | F0.3 |
| **Arch** | §7 |

- `(storefront)/error.tsx` and `(storefront)/not-found.tsx`.
- **Do not add `forbidden.tsx` yet.** Nothing calls `forbidden()` until Phase 3, and an untriggered boundary is anti-pattern #14. Add it in F3.4, in the same slice as its trigger.

**Done when** a thrown error in a storefront route renders the boundary with a working `reset()`.

---

### F0.7 — Theme toggle ✅

| | |
|---|---|
| **Goal** | The header's theme button actually switches the theme. |
| **Depends** | — |
| **Arch** | §13 Tier 6, §2 |

- `next-themes` provider with `attribute="class"` — the CSS uses `@custom-variant dark (&:is(.dark *))`.
- `suppressHydrationWarning` on `<html>`.
- Handle the pre-mount state so the icon does not flash the wrong way.

**Done when** the toggle persists across reloads and there is no flash of the wrong theme.

> The button currently renders and does nothing. A control that lies to the user is worse than no control — this slice or delete the button.

---

## Phase 1 — Catalog

**Goal:** a visitor can find and inspect any product. Everything here is public, cacheable, and works logged out.

This is where the horizontal infrastructure finally gets its first real consumer.

---

### F1.1 — Product domain foundation ✅

| | |
|---|---|
| **Goal** | One product's data can be fetched from the real API and rendered. |
| **Depends** | F0.1, F0.4 |
| **Module** | `modules/products` |
| **Layers** | types → routes → tags → queries |
| **Arch** | §24 steps 1–2, §11.3, §11.4 |

- `modules/products/types/product.ts` — the entity and its DTOs, matching the .NET API contract.
- Extend `api-routes.ts` with `products.bySlug`; extend `cache-tags.ts` with `products.bySlug`.
- `modules/products/queries/get-products.ts` — `import "server-only"`, tagged, returns `Result<Paginated<Product>>`.

**Done when** a query returns real data from the API, tagged. Verify with a temporary render, then fold that render into F1.2.

> Per the contract, this slice does not merge alone — it merges **with F1.2**, which is its consumer. Splitting them recreates exactly the problem this document exists to fix.

---

### F1.2 — Home page on real data ✅

| | |
|---|---|
| **Goal** | The home page shows real featured products from the API. |
| **Depends** | F1.1 |
| **Route** | `/[locale]` |
| **Arch** | §4 (ISR, `revalidate = 300`), §11.4, §17 |

- `modules/products/containers/featured-products-container.tsx` — fetches, handles `Result.error`, composes.
- `page.tsx` stays ~10 lines and renders containers only.
- `export const revalidate = 300`.
- `loading.tsx` with a skeleton matching the grid's `aspect-square`, so nothing shifts.
- **Delete `lib/mock/home.ts` and `public/products/`.**
- Real `generateMetadata`.

**Done when** the home page renders API products, the mock module is gone, and the skeleton causes no layout shift.

---

### F1.3 — Category listing page ⬜

| | |
|---|---|
| **Goal** | A visitor can browse all products in a category. |
| **Depends** | F1.1 |
| **Module** | `modules/categories`, `modules/products` |
| **Route** | `/[locale]/c/[slug]` |
| **Arch** | §4, §7, §16.3, §17 |

- `modules/categories/` — types, query, tags.
- ISR + `generateStaticParams` for top categories.
- Handle: category not found → `notFound()`; no products → `<EmptyState>`; fetch failure → `<ErrorState>`.
- `generateMetadata` with canonical + `alternates.languages`.

**Done when** a real category URL renders its products, an unknown slug renders not-found, and an empty category renders the empty state.

---

### F1.4 — Product detail page ⬜

| | |
|---|---|
| **Goal** | A visitor can see everything about one product. |
| **Depends** | F1.1 |
| **Route** | `/[locale]/p/[slug]` |
| **Arch** | §4, §16.3, §17 |

- Gallery, description, price via `<Money>`, variant selector (presentational for now — add-to-cart is F2.2).
- **Stock rendered from an uncached or short-TTL source** (§16.3). Do not bake availability into the ISR page.
- JSON-LD `Product` + `Offer` with price, currency, availability.
- `priority` on the hero image only.

**Done when** the page renders, structured data validates in Google's Rich Results Test, and stock is not served from the ISR cache.

---

### F1.5 — Catalog list controls ⬜

| | |
|---|---|
| **Goal** | A visitor can filter, sort and page through a category, and share that exact view as a URL. |
| **Depends** | F1.3 |
| **Arch** | §13 Tier 1, §14 (pagination rules), §16.3 |

- `nuqs` for URL state. Parameter names come from `lib/constants/query-params.ts` — `page`, `perPage`, `sort`, `q`. **No new vocabulary.**
- **One** pagination component, in `components/common/`. Not one per section.
- **Coordinate URL writes through one provider** — independent debounced writers race and silently revert each other (§13 Tier 1).
- Any filter change resets `page` to 1.
- Invalid filter values in the URL are ignored, never crash (§16.3).

**Done when** a filtered, sorted, paginated URL survives a refresh, a back button, and being pasted into another browser — and `?page=abc` renders page 1 instead of throwing.

---

### F1.6 — Search ⬜

| | |
|---|---|
| **Goal** | A visitor can search and reach results from the header on any page. |
| **Depends** | F1.5 |
| **Route** | `/[locale]/search` |
| **Arch** | §4 (dynamic), §13 Tier 1 |

Two stages, in order:

1. **Submit-only.** The header input in a `<form>` that navigates to `/search?q=…`. Zero JavaScript. Ship this first.
2. **Autocomplete.** `command` + `popover`, debounced, with `AbortController` so a slow earlier response cannot overwrite a newer one.

`httpFetcher` is `server-only`, so stage 2 needs a Route Handler or a Server Action — not a client fetch to the business API (golden rule 9).

**Done when** stage 1 works with JS disabled, and stage 2 never displays results from a superseded query.

---

### F1.7 — Category navigation ⬜

| | |
|---|---|
| **Goal** | The "All categories" button opens the real category tree. |
| **Depends** | F1.3 |
| **Arch** | §11.4, §15 |

- Fetched server-side, cached with `CacheTags.categories.list`.
- `dropdown-menu` on desktop, `sheet` on mobile (`use-mobile` already exists).
- `navigation-menu` only if the design needs a real mega-menu — it is not installed.

**Done when** the menu lists real categories and each link reaches its category page.

---

### F1.8 — Catalog SEO surface ⬜

| | |
|---|---|
| **Goal** | The catalog is fully indexable. |
| **Depends** | F1.3, F1.4 |
| **Arch** | §17 |

- `sitemap.ts` generated from the catalog, paginated if large.
- `robots.ts` disallowing `/cart`, `/checkout`, `/account`, `/admin`, and faceted-filter URL space.
- `alternates.languages` + `x-default` on every public route.
- Organization JSON-LD on the home page; `BreadcrumbList` on category pages.

**Done when** the sitemap lists real URLs and no personalized route is crawlable.

---

## Phase 2 — Cart

**Goal:** a visitor — logged out — can build a cart that survives refresh.

> [§10.1](./WEB_ARCHITECTURE.md#101-two-audiences): the storefront must work fully logged-out up to payment. Anything forcing a login to browse or add to cart is a conversion bug. The cart is deliberately built **before** identity.

---

### F2.1 — Server-owned cart session ⬜

| | |
|---|---|
| **Goal** | A visitor has a cart, identified by an httpOnly cookie, created on demand. |
| **Depends** | F0.4 |
| **Module** | `modules/cart` |
| **Arch** | §16.2, §13 (special) |

- `getOrCreateCartId()` in `modules/cart/actions/cart-session.ts`, `import "server-only"`.
- Cookie: `httpOnly`, `secure` in production, `sameSite: "lax"`, 30-day `maxAge`.
- Extend `api-routes` and `cache-tags` with the cart keys **this slice uses**.

**Done when** a first visit creates a cart id, and a refresh reuses it. **No cart data in `localStorage`, ever.**

---

### F2.2 — Add to cart ⬜

| | |
|---|---|
| **Goal** | A visitor can add a product from the detail page and see the header count change instantly. |
| **Depends** | F2.1, F1.4 |
| **Arch** | §16.2, §11.5, §1 rules 2/3 |

- Server Action with the five mandatory steps of [§11.5](./WEB_ARCHITECTURE.md#115-mutations--server-actions).
- **The schema has no price field.** The backend resolves price and validates stock. Golden rule 3.
- `useOptimistic` + `useTransition` on the button — the smallest possible client leaf.
- Failure → toast via the client `translateApiError` adapter; React reverts the optimistic value automatically.
- `revalidateTag(CacheTags.cart.byId(cartId))`.

**Done when** the count updates before the round trip completes, and a forced server failure reverts it and shows a translated message.

---

### F2.3 — Cart page ⬜

| | |
|---|---|
| **Goal** | A visitor can review the cart, change quantities, and remove lines. |
| **Depends** | F2.2 |
| **Route** | `/[locale]/cart` |
| **Arch** | §16.2, §4, anti-pattern #20 |

- **`export const dynamic = "force-dynamic"`.** Caching this leaks another customer's cart — the most severe failure in §23.
- Update-quantity and remove-line Server Actions, both optimistic.
- **Every total comes from the server.** No client-side arithmetic on line items.
- Empty-cart state with a route back into the catalog.

**Done when** the page is never cached, totals always match the server, and the empty state renders.

---

### F2.4 — Cart summary in the header ⬜

| | |
|---|---|
| **Goal** | The badge shows the real count on every page; the mini-cart opens without navigating. |
| **Depends** | F2.2 |
| **Arch** | §4 (children-as-slot), §19 |

- The count is read on the server. Do **not** convert the header to a Client Component for it — pass the server-rendered subtree as `children` (anti-pattern #1).
- Mini-cart in a `sheet`.

**Done when** the badge is accurate across navigations and the header ships no more JS than the toggle and the sheet require.

---

## Phase 3 — Identity

**Goal:** a customer can have an account — without any of Phase 1 or 2 having required one.

---

### F3.1 — Auth providers, session and refresh ⬜

| | |
|---|---|
| **Goal** | A customer can sign in and the session stays valid without them noticing. |
| **Depends** | — |
| **Arch** | §10.2, §10.3, §2 |

- `config/auth/` — options, callbacks, `types.d.ts` augmentation.
- Session carries: tokens with UTC ISO expiry, minimal user, permissions. Nothing more — it travels in a cookie on every request.
- Proactive refresh in the JWT callback with **one** `REFRESH_SKEW_MS` constant.
- If you declare `session.error`, you must set it **and** handle it (anti-pattern #8).
- Complete [F0.4](#f04--auth-aware-fetcher-): inject the token.

**Done when** a session refreshes before expiry and a dead refresh token produces a handled `RefreshFailed`, not a silent failure.

---

### F3.2 — Auth routes ⬜

| | |
|---|---|
| **Goal** | A customer can register, sign in, sign out, and reset a password. |
| **Depends** | F3.1 |
| **Route** | `/[locale]/auth/*` — **outside** `(account)`, or the guard loops |
| **Arch** | §7, §12 |

- First use of the form stack: two schemas per entity, schemas as factories, one `useForm` per form, provider owns submit ([§12](./WEB_ARCHITECTURE.md#12-forms)).
- `lib/schemas/` for reusable fragments (email, password).

**Done when** all four flows work and validation messages are translated in both locales.

---

### F3.3 — Session guards ⬜

| | |
|---|---|
| **Goal** | Protected sections are unreachable without a valid session. |
| **Depends** | F3.1, F0.5 |
| **Arch** | §9, §10.4 |

- Auth interceptor into the pipeline, **before** intl, excluding `/auth/*`.
- **Fails closed.** Swallowing an exception here turns a crash into an open door (anti-pattern #18).
- Server guard on every render; client expiry watcher at 30–60s, not 5s.

**Done when** a revoked session is caught on the next navigation and an idle tab notices expiry.

---

### F3.4 — Permission predicate ⬜

| | |
|---|---|
| **Goal** | One authorization check, used by every consumer. |
| **Depends** | F3.1 |
| **Arch** | §10.5, §7 |

- `lib/auth/permissions.ts` — pure `can()`, no React, no server APIs.
- `usePermissions()` (client) and `requirePermission()` (server) both built on it. **Three copies drift into a security bug.**
- Add `forbidden.tsx` **now**, in the same slice as the `forbidden()` call that triggers it.
- Memoize session lookups per request (anti-pattern #19).

**Done when** exactly one `.some(...)` predicate exists in the codebase and `forbidden.tsx` is reachable.

---

### F3.5 — Cart merge on login ⬜

| | |
|---|---|
| **Goal** | A guest cart survives signing in. |
| **Depends** | F2.1, F3.1 |
| **Arch** | §16.2 |

- On successful sign-in, merge the guest cart into the persistent cart **server-side**, summing quantities and re-validating stock. Clear the guest cookie.

**Done when** a guest builds a cart, signs in, and loses nothing.

> §16.2 flags this explicitly: implement it, or you will lose carts at the worst possible moment.

---

### F3.6 — Account menu ⬜

| | |
|---|---|
| **Goal** | The avatar reflects the real session. |
| **Depends** | F3.1 |
| **Arch** | §4 |

- Read the session with `auth()` on the server, not `useSession()` — less JS and no logged-out flash.
- `dropdown-menu` for the account actions.

**Done when** the header shows real identity with no flash of the signed-out state.

---

## Phase 4 — Checkout and orders

**Goal:** a customer — guest or authenticated — can pay and receive an order.

> [§16.4](./WEB_ARCHITECTURE.md#164-checkout): the highest-stakes flow in the app. Every slice here needs tests before merge (§20).

---

### F4.1 — Checkout shell and steps ⬜

| | |
|---|---|
| **Goal** | A customer moves through contact → shipping → payment with per-step validation. |
| **Depends** | F2.3 |
| **Route** | `/[locale]/checkout/*` — `force-dynamic` + `noindex` |
| **Arch** | §12.5, §13 Tier 4/5, §16.4 |

- **One form state, several steps.** `form.trigger(stepFields[step])` per step — not one `useForm` per step.
- Cross-navigation drafts in `sessionStorage`, keyed, cleared on success. Keys in `lib/constants/storage-keys.ts`. **Name the folder after the storage it actually uses** (anti-pattern #11).

**Done when** a customer can leave to add an address and return without losing input.

---

### F4.2 — Addresses ⬜ · F4.3 — Shipping methods ⬜

| | |
|---|---|
| **Depends** | F4.1 |
| **Arch** | §12.1, §16.4 |

Address form with `.superRefine()` for conditional requirements (shipping vs pickup) rather than a wizard. Shipping options and cost resolved **server-side** from the address.

**Done when** an unserviceable address is rejected with a recovery path, and shipping cost never comes from the client.

---

### F4.4 — Payment ⬜

| | |
|---|---|
| **Depends** | F4.3 |
| **Arch** | §16.4, §16.5 |

- **Never store raw card data.** Provider-tokenized element or redirect. PCI scope is not acquired by accident.
- Explicit UI for: declined, stock disappeared mid-checkout, address rejected, session expired — each with its own recovery path.

**Done when** every one of those four failures has a distinct, tested message and route forward.

---

### F4.5 — Order placement ⬜

| | |
|---|---|
| **Depends** | F4.4 |
| **Arch** | §16.4, §16.6, §20 |

- **Client-generated idempotency key.** A retry with the same key returns the original order, never a second one.
- Server re-validates **everything** at submit: availability, current prices, discount eligibility, shipping validity, address serviceability. Nothing carries over on trust.
- Stock reserved at placement, not at add-to-cart.
- Oversell race handled atomically: one success, one clear recoverable failure.

**Done when** a double-submit produces one order, and a concurrent last-unit purchase produces exactly one success. **Both covered by tests.**

---

### F4.6 — Payment webhook ⬜

| | |
|---|---|
| **Depends** | F4.5 |
| **Route** | `app/api/webhooks/payments/route.ts` |
| **Arch** | §16.5, §11.6 |

- **The webhook is the source of truth for order state**, not the browser redirect.
- Verify the signature **on the raw body, before parsing**.
- Idempotent — providers retry and duplicates are normal.
- Respond `2xx` fast; enqueue slow work.

**Done when** a customer who closes the tab immediately after paying still gets their order, and a replayed webhook changes nothing.

---

### F4.7 — Order confirmation ⬜ · F4.8 — Guest checkout ⬜

| | |
|---|---|
| **Depends** | F4.5 |
| **Arch** | §16.4, §10.1 |

Confirmation page reading verified server state — never an amount or status from a redirect parameter. **Guest checkout works**; account creation is offered *after* the order, never required before it.

**Done when** a visitor completes a purchase having never signed in.

---

## Phase 5 — Customer account

**Goal:** a customer can see and manage their own data — and only their own.

---

### F5.1 — Account shell ⬜

Route group `(account)`, `force-dynamic`, `noindex`, auth-gated by F3.3. Nav + layout.

### F5.2 — Order history and detail ⬜

**Depends** F4.5 · **Arch** §10.6

Scoped by the session's customer id **on the server**. Never accept a `customerId` from the client — for a read or a write.

**Done when** manipulating an order id in the URL returns not-found, not another customer's order.

### F5.3 — Address book ⬜ · F5.4 — Profile ⬜

**Depends** F4.2 / F3.2 · Reuse the address schema from F4.2. Do not write a second one (anti-pattern #6).

### F5.5 — Wishlist ⬜

**Depends** F3.1 · **Arch** §16.2 pattern

Makes the product card's heart button real. Server-owned, optimistic, same shape as add-to-cart.

> Like the theme toggle: the heart currently renders and does nothing. Ship this or remove it.

---

## Phase 6 — Back office

**Goal:** staff can run the store.

> [§3](./WEB_ARCHITECTURE.md#3-repository-shape): stays in the same app under `(admin)`. Next code-splits per route, so no admin JS reaches a storefront visitor. Split into a separate app only when one of the three named conditions is true.

---

### F6.1 — Admin shell ⬜

**Depends** F3.4 · **Arch** §7, §10.4, §10.5

Route group, two-layer guard, `routes/navigation-data.ts` with the permission each entry requires. Sidebar filtered by `can()`.

### F6.2 — The data table ⬜

**Depends** F6.1 · **Arch** §14

**One** table system in `components/data-table/`. URL-driven pagination, sorting and filtering (Tier 1) — the table holds no list state. Server-side pagination. Permission-gated row actions. Explicit empty, skeleton and error states.

> Build it once, here. §14 is unambiguous: do not build a second one. Reuse the pagination component from F1.5.

### F6.3 — Products CRUD ⬜

**Depends** F6.2, F1.1 · **Arch** §24, §7, §12

The first full run of the [§24 recipe](./WEB_ARCHITECTURE.md#24-recipe-scaffold-a-new-module) — all twelve steps, four routes, two schemas, one provider. **Mutations `revalidateTag` the storefront's product tags**, closing the loop with Phase 1.

**Done when** editing a product in admin updates its storefront page without a deploy.

### F6.4 — Categories ⬜ · F6.5 — Orders ⬜ · F6.6 — Customers ⬜ · F6.7 — Discounts ⬜ · F6.8 — Inventory ⬜

**Depends** F6.3 (as the template)

Each repeats the §24 recipe. Orders needs status transitions and refunds; discounts needs rule validation with tests (§20 — money); inventory needs stock adjustment with the same atomicity guarantees as F4.5.

---

## Phase 7 — Production readiness

Not last in importance — last in dependency. **Pull F7.1 and F7.4 forward** as soon as Phase 1 lands; both get harder the longer they wait.

### F7.1 — Testing harness ⬜
**Arch** §20 · Vitest for pure logic and Server Actions; Testing Library for components; Playwright for browse → cart → checkout → confirmation, guest **and** authenticated.
Minimum bar for any PR touching money, cart or checkout: unit tests for the calculation plus an E2E covering happy path and one failure path.

### F7.2 — Observability ⬜
**Arch** §18 · Sentry per runtime (server, edge, browser). Report from **every** error boundary. `NEXT_PUBLIC_*` for the browser DSN — verify in a real build. Scrub PII in `beforeSend`. Instrument add-to-cart failures, checkout drop-off, payment errors.

### F7.3 — Environment validation ⬜
**Arch** §21 · Validate required env vars at startup, fail fast naming the missing variable. `.env.example` documents every one.

### F7.4 — Performance budgets ⬜
**Arch** §19 · LCP ≤ 2.5s, CLS ≤ 0.1, INP ≤ 200ms, first-load JS ≤ 130KB gzipped on storefront routes. Enforced in CI, failing on regression.

### F7.5 — Build and deploy ⬜
**Arch** §21 · Multi-stage Docker with `turbo prune`, non-root user, `--frozen-lockfile`, health endpoint, tested rollback path.

---

## 14. Dependency map

```
Phase 0 ── F0.1 realign ──┐
           F0.2 money     │
           F0.3 errors    ├──► F1.1 ─┬─► F1.2 home
           F0.4 fetcher ──┤          ├─► F1.3 category ─┬─► F1.5 controls ──► F1.6 search
           F0.5 pipeline ─┤          │                  └─► F1.7 nav
           F0.6 boundaries│          └─► F1.4 product ──┐
           F0.7 theme     │                             ├──► F1.8 SEO
                          │                             │
                          └──► F2.1 cart session ──► F2.2 add ─┬─► F2.3 page ──► F4.1 checkout
                                                                └─► F2.4 header
     F3.1 auth ─┬─► F3.2 routes                                              │
                ├─► F3.3 guards ──► F6.1 admin shell ──► F6.2 table ──► F6.3 products ──► F6.4…F6.8
                ├─► F3.4 permissions ─┘                                      │
                ├─► F3.5 cart merge                                          │
                └─► F3.6 menu                                                ▼
                                        F4.2 address ─► F4.3 shipping ─► F4.4 payment ─► F4.5 order
                                                                                          │
                                                          F4.6 webhook ◄──────────────────┤
                                                          F4.7 confirmation ◄─────────────┤
                                                          F5.2 order history ◄────────────┘
```

**Critical path to a store that can take money:**
`F0.1 → F0.4 → F1.1 → F1.4 → F2.1 → F2.2 → F2.3 → F4.1 → F4.4 → F4.5 → F4.6`

Everything else can be sequenced around it.

---

## 15. Conventions this document adds

Three rules that only make sense once delivery is vertical. They extend the architecture; they never override it.

**1. A catalog key ships with its caller.**
Adding `apiRoutes.orders.byId` without a query that calls it is the horizontal failure in miniature. This makes it a review-blocking rule rather than a habit.

**2. A slice that produces no reachable UI must merge with the slice that consumes it.**
[F1.1](#f11--product-domain-foundation-) is the canonical case: a query with no page is not a deliverable.

**3. A rendered control must do what it appears to do.**
A theme toggle that does not toggle, a heart that saves nothing — these are anti-pattern #14 wearing a different hat. Either wire it in the slice that introduces it, or leave it out of the markup.

### Resolving one ambiguity in the architecture

[§5](./WEB_ARCHITECTURE.md#5-directory-structure)'s module anatomy omits `queries/`, but [§11.4](./WEB_ARCHITECTURE.md#114-queries--server-components), [§22](./WEB_ARCHITECTURE.md#22-naming-and-code-conventions) and [§24](./WEB_ARCHITECTURE.md#24-recipe-scaffold-a-new-module) all use it. **`queries/` is canonical** — §5's list is incomplete. Per §5's own closing rule, fix §5 in the PR that proves it, rather than carrying the ambiguity.

---

## Keeping this document honest

Update the status marker **in the PR that changes it**, never in a batch afterwards. A roadmap edited retroactively is fiction.

When a slice reveals that a rule here is wrong, change it in the same PR that proves it wrong — same standard `WEB_ARCHITECTURE.md` sets for itself. A tracking document nobody trusts is worse than none.
