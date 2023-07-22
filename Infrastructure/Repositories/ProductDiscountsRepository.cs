using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ProductDiscountsRepository : GenericRepository<ProductDiscounts, ProductDiscountsFilter>, IProductDiscountsRepository
    {
        protected readonly string _tableDiscountName;
        public ProductDiscountsRepository(IDBFactory context) : base(context)
        {
            _tableDiscountName = typeof(ProductDiscounts).Name;
        }

        public Task<IEnumerable<ProductDiscounts>> GetProductDiscountByProduct(int productId, string dbName)
        {
            throw new NotImplementedException();
        }
    }
}
