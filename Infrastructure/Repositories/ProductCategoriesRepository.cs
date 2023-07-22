using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ProductCategoriesRepository : GenericRepository<ProductCategories, ProductCategoriesFilter>, IProductCategoriesRepository
    {
        protected readonly string _tableCategoryName;
        public ProductCategoriesRepository(IDBFactory context) : base(context)
        {
            _tableCategoryName = typeof(ProductCategories).Name;
        }
    }
}
