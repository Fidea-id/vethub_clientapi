using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
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

        public async Task GenerateTableField(string dbName)
        {
            try
            {
                var filePath = $"{Directory.GetCurrentDirectory()}\\InitData.json";
                if (File.Exists(filePath))
                {
                    // Read the contents of the file
                    string json = File.ReadAllText(filePath);

                    // Deserialize the JSON data into an object
                    var jsonData = JsonConvert.DeserializeObject(json);

                    //check the schema data
                    var check = await _generateTableRepository.CheckInitSchema(dbName, "initdata_1");
                    if (!check)
                    {
                        var deserializedObjects = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                        if (deserializedObjects != null)
                        {
                            foreach (var kvp in deserializedObjects)
                            {
                                if (kvp.Key == "ProductCategories")
                                {
                                    var data = JsonConvert.DeserializeObject<IEnumerable<ProductsCategoriesRequest>>(kvp.Value.ToString());
                                    //map items
                                    var map = Mapping.Mapper.Map<IEnumerable<ProductCategories>>(data);
                                    foreach (var itm in map)
                                    {
                                        FormatUtil.TrimObjectProperties(itm);
                                        FormatUtil.SetIsActive<ProductCategories>(itm, true);
                                        FormatUtil.SetDateBaseEntity<ProductCategories>(itm);
                                    }

                                    await _unitOfWork.ProductsRepository.AddProductCategoriesRange(map, dbName);
                                }
                                else if (kvp.Key == "AppointmentsStatus")
                                {
                                    var data = JsonConvert.DeserializeObject<IEnumerable<AppointmentsStatusRequest>>(kvp.Value.ToString());
                                    //map items
                                    var map = Mapping.Mapper.Map<IEnumerable<AppointmentsStatus>>(data);
                                    foreach (var itm in map)
                                    {
                                        FormatUtil.TrimObjectProperties(itm);
                                        FormatUtil.SetIsActive<AppointmentsStatus>(itm, true);
                                        FormatUtil.SetDateBaseEntity<AppointmentsStatus>(itm);
                                    }
                                    await _unitOfWork.AppointmentRepository.AddStatusRange(map, dbName);
                                }

                            }
                            // set the schema init
                            await _generateTableRepository.SetInitSchema(dbName, "initdata_1");
                        }
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
