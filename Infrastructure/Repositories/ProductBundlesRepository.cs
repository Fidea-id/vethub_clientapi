using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ProductBundlesRepository : GenericRepository<ProductBundles, ProductBundlesFilter>, IProductBundlesRepository
    {
        protected readonly string _tableBundleName;
        public ProductBundlesRepository(IDBFactory context) : base(context)
        {
            _tableBundleName = typeof(ProductBundles).Name;
        }

        public async Task<IEnumerable<ProductBundles>> GetProductBundlesByProduct(int productId, string dbName)
        {
            throw new NotImplementedException();
        }
    }
}
