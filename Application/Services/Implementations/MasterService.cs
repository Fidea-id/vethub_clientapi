using Application.Services.Contracts;
using Domain.Interfaces.Clients;

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
                await _generateTableRepository.GenerateTableField(dbName, fields);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task GenerateTables(string dbName, int? version = null)
        {
            try
            {
                if(version.HasValue)
                {
                    await _generateTableRepository.UpdateTable(dbName, version.Value);
                }
                else
                {
                    await _generateTableRepository.GenerateAllTable(dbName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
