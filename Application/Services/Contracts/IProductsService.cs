using Domain.Entities.Filters;
using Domain.Entities.Models;
using Domain.Entities.Requests;
using Domain.Entities.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Contracts
{
    public interface IProductsService : IGenericService<Products, ProductsRequest, Products, ProductsFilter>
    {
    }
}
