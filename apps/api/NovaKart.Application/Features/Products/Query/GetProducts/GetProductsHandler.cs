using System.Linq;
using Mediator;
using NovaKart.Application.Common;
using NovaKart.Application.Dtos.Categories;
using NovaKart.Application.Dtos.Products;
using NovaKart.Domain.Interfaces;

namespace NovaKart.Application.Features.Products.Query.GetProducts
{
    public class GetProductsHandler : IRequestHandler<GetProductsQuery, Result<GetProductsResponse>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductsHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async ValueTask<Result<GetProductsResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _productRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                cancellationToken,
                request.SearchTerm,
                request.CategoryId,
                request.MinPrice,
                request.MaxPrice,
                request.IsActive);

            var products = items.Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.IsActive,
                new ProductCategoryDto(p.Category.Id, p.Category.Name)))
                .ToList();

            return new GetProductsResponse(products, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
