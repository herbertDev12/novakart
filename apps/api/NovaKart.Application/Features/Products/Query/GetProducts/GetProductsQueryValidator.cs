using FluentValidation;

namespace NovaKart.Application.Features.Products.Query.GetProducts
{
    public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
    {
        public const int MaxPageSize = 100;

        public GetProductsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, MaxPageSize);

            RuleFor(x => x.MinPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinPrice.HasValue);

            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxPrice.HasValue);
        }
    }
}
