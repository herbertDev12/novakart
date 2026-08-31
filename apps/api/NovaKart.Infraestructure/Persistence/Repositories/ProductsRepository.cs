using Microsoft.EntityFrameworkCore;
using NovaKart.Domain.Interfaces;
using NovaKart.Domain.Entities;


namespace NovaKart.Infraestructure.Persistence.Repositories
{
    public class ProductsRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Product> _products;

        public ProductsRepository(ApplicationDbContext context)
        {
            _context = context;
            _products = _context.Set<Product>();
        }

        public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _products.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken,
            string? searchTerm = null,
            Guid? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isActive = null)
        {
            var query = _products.AsNoTracking().Include(p => p.Category)
            .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                p.Description.ToLower().Contains(searchTerm.ToLower()));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}