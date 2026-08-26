using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NovaKart.Domain.Entities;

namespace NovaKart.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync(CancellationToken cancellationToken);

        Task<Product> GetProductByIdAsync(Guid Id, CancellationToken cancellationToken);

        Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken,
            string? searchTerm = null,
            Guid? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isActive = null);
    }
}