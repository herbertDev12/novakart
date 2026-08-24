import { NextRequest, NextResponse } from "next/server";

export type InterceptorFn = (
  req: NextRequest,
) => Promise<NextResponse | void> | NextResponse | void;

type Entry = { name: string; fn: InterceptorFn; matcher?: RegExp };

export class InterceptorPipeline {
  private readonly entries: Entry[] = [];

  use(name: string, fn: InterceptorFn, matcher?: string): this {
    this.entries.push({
      name,
      fn,
      matcher: matcher ? new RegExp(matcher) : undefined,
    });
    return this;
  }

  async execute(req: NextRequest): Promise<NextResponse> {
    const { pathname } = req.nextUrl;
    let response = NextResponse.next();

    for (const entry of this.entries) {
      if (entry.matcher && !entry.matcher.test(pathname)) continue;

      try {
        const result = await entry.fn(req);
        if (!result) continue;
        if (result.status >= 300) return result; // short-circuit: redirect/deny wins

        // carry forward headers and cookies set by this interceptor
        result.headers.forEach((value, key) =>
          response.headers.set(key, value),
        );
        result.cookies.getAll().forEach((c) => response.cookies.set(c));
        response = result;
      } catch (error) {
        // Fail CLOSED for anything that gates access.
        if (entry.name === "auth") throw error;
        console.error(`[interceptor:${entry.name}]`, error);
      }
    }

    return response;
  }
}
