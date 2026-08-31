using Mediator;
using NovaKart.Application.Common;
using NovaKart.Application.Dtos.Products;

namespace NovaKart.Application.Features.Products.Query.GetProducts
{
    public sealed record GetProductsQuery(
        int PageNumber,
        int PageSize,
        string? SearchTerm = null,
        Guid? CategoryId = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        bool? IsActive = null
    ) : IRequest<Result<GetProductsResponse>>;
}
