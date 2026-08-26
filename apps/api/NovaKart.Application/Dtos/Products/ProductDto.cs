using NovaKart.Application.Dtos.Categories;

namespace NovaKart.Application.Dtos.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    bool IsActive,
    ProductCategoryDto Category);

public record GetProductsResponse(
    List<ProductDto> Products,
    int TotalCount,
    int PageNumber,
    int PageSize);