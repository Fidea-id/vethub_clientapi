using Application.Services.Contracts;
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
