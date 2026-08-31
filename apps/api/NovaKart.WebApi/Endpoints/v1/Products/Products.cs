using Mediator;
using NovaKart.Application.Features.Products.Query.GetProducts;
using NovaKart.WebApi.Common;

namespace NovaKart.WebApi.Endpoints.v1.Products
{
    public static class Products
    {
        public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/products", async (
                IMediator mediator,
                CancellationToken cancellationToken,
                int pageNumber = 1,
                int pageSize = 20,
                string? searchTerm = null,
                Guid? categoryId = null,
                decimal? minPrice = null,
                decimal? maxPrice = null,
                bool? isActive = null) =>
            {
                var query = new GetProductsQuery(pageNumber, pageSize, searchTerm, categoryId, minPrice, maxPrice, isActive);
                var result = await mediator.Send(query, cancellationToken);
                return result.ToHttpResult();
            })
            .WithName("GetProducts")
            .WithTags("Products");

            return app;
        }
    }
}
