using Application.Services.Contracts;
using Domain.Entities.Models;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class MasterService : IMasterService
    {
        private readonly IGenerateTableRepository _generateTableRepository;

        public MasterService(IGenerateTableRepository generateTableRepository)
        {
            _generateTableRepository = generateTableRepository;
        }

        public async Task GenerateTableField(string dbName, Dictionary<string, object> fields)
        {
            try
            {
                //var fieldss = new Dictionary<string, object>
                //{
                //    {
                //        "ProductCategories", new List<ProductCategories>
                //        {
                //            new ProductCategories { Name = "Food" },
                //            new ProductCategories { Name = "Drink" },
                //            new ProductCategories { Name = "Accessories" }
                //        }
                //    }
                //};
                await _generateTableRepository.GenerateTableField(dbName, fields);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task GenerateTables(string dbName)
        {
            try
            {
                await _generateTableRepository.GenerateAllTable(dbName);
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
