using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
