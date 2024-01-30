using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Services.Implementations
{
    public class MasterService : IMasterService
    {
        private readonly IGenerateTableRepository _generateTableRepository;
        private ILogger<MasterService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public MasterService(IGenerateTableRepository generateTableRepository, IUnitOfWork unitOfWork,
            ILoggerFactory loggerFactory)
        {
            _generateTableRepository = generateTableRepository;
            _unitOfWork = unitOfWork;
            _logger = loggerFactory.CreateLogger<MasterService>();
        }

        public async Task GenerateTableField(string dbName)
        {
            try
            {
                var filePath = $"{Directory.GetCurrentDirectory()}/wwwroot/DataInsert/initData.json";
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
                                    _logger.LogInformation("Try to map " + kvp.Key);
                                    var data = JsonConvert.DeserializeObject<IEnumerable<ProductsCategoriesRequest>>(kvp.Value.ToString());
                                    //map items
                                    var map = Mapping.Mapper.Map<IEnumerable<ProductCategories>>(data);
                                    foreach (var itm in map)
                                    {
                                        FormatUtil.TrimObjectProperties(itm);
                                        FormatUtil.SetIsActive<ProductCategories>(itm, true);
                                        FormatUtil.SetDateBaseEntity<ProductCategories>(itm);
                                    }
                                    _logger.LogInformation("Success map " + kvp.Key);

                                    await _unitOfWork.ProductCategoriesRepository.AddRange(dbName, map);
                                }
                                else if (kvp.Key == "AppointmentsStatus")
                                {
                                    _logger.LogInformation("Try to map " + kvp.Key);
                                    var data = JsonConvert.DeserializeObject<IEnumerable<AppointmentsStatusRequest>>(kvp.Value.ToString());
                                    //map items
                                    var map = Mapping.Mapper.Map<IEnumerable<AppointmentsStatus>>(data);
                                    foreach (var itm in map)
                                    {
                                        FormatUtil.TrimObjectProperties(itm);
                                        FormatUtil.SetIsActive<AppointmentsStatus>(itm, true);
                                        FormatUtil.SetDateBaseEntity<AppointmentsStatus>(itm);
                                    }
                                    _logger.LogInformation("Success map " + kvp.Key);
                                    await _unitOfWork.AppointmentRepository.AddStatusRange(map, dbName);
                                }
                                else if (kvp.Key == "PaymentMethod")
                                {
                                    _logger.LogInformation("Try to map " + kvp.Key);
                                    var data = JsonConvert.DeserializeObject<IEnumerable<PaymentMethodRequest>>(kvp.Value.ToString());
                                    //map items
                                    var map = Mapping.Mapper.Map<IEnumerable<PaymentMethod>>(data);
                                    foreach (var itm in map)
                                    {
                                        FormatUtil.TrimObjectProperties(itm);
                                        FormatUtil.SetIsActive<PaymentMethod>(itm, true);
                                        FormatUtil.SetDateBaseEntity<PaymentMethod>(itm);
                                    }
                                    _logger.LogInformation("Success map " + kvp.Key);
                                    await _unitOfWork.PaymentMethodRepository.AddRange(dbName, map);
                                }
                                else if (kvp.Key == "Animals")
                                {
                                    _logger.LogInformation("Try to map " + kvp.Key);
                                    var data = JsonConvert.DeserializeObject<IEnumerable<AnimalsRequest>>(kvp.Value.ToString());
                                    //map items
                                    var map = Mapping.Mapper.Map<IEnumerable<Animals>>(data);
                                    foreach (var itm in map)
                                    {
                                        FormatUtil.TrimObjectProperties(itm);
                                        FormatUtil.SetIsActive<Animals>(itm, true);
                                        FormatUtil.SetDateBaseEntity<Animals>(itm);
                                    }
                                    _logger.LogInformation("Success map " + kvp.Key);
                                    await _unitOfWork.AnimalRepository.AddRange(dbName, map);
                                }
                                else if (kvp.Key == "Breeds")
                                {
                                    _logger.LogInformation("Try to map " + kvp.Key);
                                    var data = JsonConvert.DeserializeObject<IEnumerable<BreedsRequest>>(kvp.Value.ToString());
                                    //map items
                                    var map = Mapping.Mapper.Map<IEnumerable<Breeds>>(data);
                                    foreach (var itm in map)
                                    {
                                        FormatUtil.TrimObjectProperties(itm);
                                        FormatUtil.SetIsActive<Breeds>(itm, true);
                                        FormatUtil.SetDateBaseEntity<Breeds>(itm);
                                    }
                                    _logger.LogInformation("Success map " + kvp.Key);
                                    await _unitOfWork.BreedRepository.AddRange(dbName, map);
                                }
                                else if (kvp.Key == "Services")
                                {
                                    _logger.LogInformation("Try to map " + kvp.Key);
                                    var data = JsonConvert.DeserializeObject<IEnumerable<ServicesRequest>>(kvp.Value.ToString());
                                    //map items
                                    var map = Mapping.Mapper.Map<IEnumerable<Domain.Entities.Models.Clients.Services>>(data);
                                    foreach (var itm in map)
                                    {
                                        FormatUtil.TrimObjectProperties(itm);
                                        FormatUtil.SetIsActive<Domain.Entities.Models.Clients.Services>(itm, true);
                                        FormatUtil.SetDateBaseEntity<Domain.Entities.Models.Clients.Services>(itm);
                                    }
                                    _logger.LogInformation("Success map " + kvp.Key);
                                    await _unitOfWork.ServicesRepository.AddRange(dbName, map);
                                }
                                else if (kvp.Key == "Diagnoses")
                                {
                                    _logger.LogInformation("Try to map " + kvp.Key);
                                    var data = JsonConvert.DeserializeObject<IEnumerable<DiagnosesRequest>>(kvp.Value.ToString());
                                    //map items
                                    var map = Mapping.Mapper.Map<IEnumerable<Diagnoses>>(data);
                                    foreach (var itm in map)
                                    {
                                        FormatUtil.TrimObjectProperties(itm);
                                        FormatUtil.SetIsActive<Diagnoses>(itm, true);
                                        FormatUtil.SetDateBaseEntity<Diagnoses>(itm);
                                    }
                                    _logger.LogInformation("Success map " + kvp.Key);
                                    await _unitOfWork.DiagnoseRepository.AddRange(dbName, map);
                                }
                                //else if (kvp.Key == "PrescriptionFrequents")
                                //{
                                //    _logger.LogInformation("Try to map " + kvp.Key);
                                //    var data = JsonConvert.DeserializeObject<IEnumerable<PrescriptionFrequentsRequest>>(kvp.Value.ToString());
                                //    //map items
                                //    var map = Mapping.Mapper.Map<IEnumerable<PrescriptionFrequents>>(data);
                                //    foreach (var itm in map)
                                //    {
                                //        FormatUtil.TrimObjectProperties(itm);
                                //        FormatUtil.SetIsActive<PrescriptionFrequents>(itm, true);
                                //        FormatUtil.SetDateBaseEntity<PrescriptionFrequents>(itm);
                                //    }
                                //    _logger.LogInformation("Success map " + kvp.Key);
                                //    await _unitOfWork.PrescriptionFrequentsRepository.AddRange(dbName, map);
                                //}

                            }
                            // set the schema init
                            await _generateTableRepository.SetInitSchema(dbName, "initdata_2");
                        }
                    }
                }
                else
                {
                    throw new Exception($"File not found, {filePath}");
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
                if (version.HasValue)
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
