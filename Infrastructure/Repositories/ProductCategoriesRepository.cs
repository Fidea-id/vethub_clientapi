using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;
using static Dapper.SqlMapper;

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
