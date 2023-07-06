using Application.Services.Contracts;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;
using Newtonsoft.Json;

namespace Application.Services.Implementations
{
    public class MasterService : IMasterService
    {
        private readonly IGenerateTableRepository _generateTableRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MasterService(IGenerateTableRepository generateTableRepository, IUnitOfWork unitOfWork)
        {
            _generateTableRepository = generateTableRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task GenerateTableField(string dbName, string json)
        {
            try
            {
                var deserializedObjects = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                foreach (var kvp in deserializedObjects)
                {
                    if (kvp.Key == "ProductCategories")
                    {
                        var data = JsonConvert.DeserializeObject<IEnumerable<ProductsCategoriesRequest>>(kvp.Value.ToString());
                        //await _unitOfWork.ProductsRepository.AddProductCategoriesRange(data, dbName);
                    }
                    else if (kvp.Key == "Services")
                    {
                        var data = JsonConvert.DeserializeObject<IEnumerable<ServicesRequest>>(kvp.Value.ToString());
                        //await _unitOfWork.ServicesRepository.AddRange(dbName, data);
                    }
                }
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
