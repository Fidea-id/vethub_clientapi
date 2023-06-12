using Application.Services.Contracts;
using Domain.Entities.Filters;
using Domain.Entities.Models;
using Domain.Entities.Requests;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class ProductsService : GenericService<Products, ProductsRequest, Products, ProductsFilter>, IProductsService
    {
        public ProductsService(IUnitOfWork unitOfWork, IGenericRepository<Products, ProductsFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}
