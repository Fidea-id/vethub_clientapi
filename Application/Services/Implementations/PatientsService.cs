using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;

namespace Application.Services.Implementations
{
    public class PatientsService : GenericService<Patients, PatientsRequest, Patients, PatientsFilter>, IPatientsService
    {
        public PatientsService(IUnitOfWork unitOfWork, IGenericRepository<Patients, PatientsFilter> repository, ICurrentUserService currentUser)
        : base(unitOfWork, repository, currentUser)
        { }

        public async Task<IEnumerable<Patients>> ReadByOwnerIdAsync(int id, string dbName)
        {
            return await _unitOfWork.PatientsRepository.GetPatientsByOwner(dbName, id);
        }

        public async Task<DataResultDTO<PatientsListResponse>> ReadPatientsList(PatientsFilter filter, string dbName)
        {
            return await _unitOfWork.PatientsRepository.GetPatientsList(dbName, filter);
        }

        public async Task<PatientsStatistic> AddPatientStatistic(PatientsStatisticRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<PatientsStatistic>(request);
                FormatUtil.SetIsActive<PatientsStatistic>(entity, true);
                FormatUtil.SetDateBaseEntity<PatientsStatistic>(entity);
                switch (entity.Type)
                {
                    case "Blood Pressure":
                        entity.Unit = "mmHg";
                        break;
                    case "Weight":
                        entity.Unit = "Kg";
                        break;
                    case "Temperature":
                        entity.Unit = "C";
                        break;
                    default:
                        entity.Unit = "";
                        break;
                }

                var newId = await _unitOfWork.PatientsStatisticRepository.Add(dbName, entity);
                entity.Id = newId;

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "AddPatientStatistic", MethodType.Create, nameof(PatientsStatistic));
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"PatientService.AddPatientStatistic";
                throw;
            }
        }

        public async Task<IEnumerable<PatientsStatisticResponse>> ReadPatientsStatisticAsync(int patientId, string dbName)
        {
            var dtoData = await _unitOfWork.PatientsStatisticRepository.ReadPatientsStatisticAsync(patientId, dbName);

            var result = new List<PatientsStatisticResponse>();
            foreach (var item in dtoData)
            {
                string change;
                string beforeValueText = item.Before != null ? $"{item.Before} {item.Unit}" : "null";
                if (item.Type == "Blood Pressure")
                {
                    change = "";
                }
                else if (item.Before == null)
                {
                    change = null;
                }
                else
                {
                    double latestValue = double.Parse(item.Latest);
                    double beforeValue = double.Parse(item.Before);
                    double changeValue = latestValue - beforeValue;
                    double changePercentage = (latestValue / beforeValue) * 100;
                    change = $"{changeValue} ({changePercentage.ToString("0.00")}%)";
                }
                var checkedAt = FormatUtil.GetTimeAgo(item.CreatedAt);
                var staff = await _unitOfWork.ProfileRepository.GetById(dbName, item.StaffId);
                var newResponse = new PatientsStatisticResponse()
                {
                    Latest = $"{item.Latest} {item.Unit}",
                    Before = beforeValueText,
                    Change = change,
                    PatientId = item.PatientId,
                    Type = item.Type,
                    CheckedAt = checkedAt,
                    CheckedBy = staff.Name
                };
                result.Add(newResponse);
            }
            return result;
        }

        public Task<IEnumerable<PatientsStatisticHistoryResponse>> ReadPatientsStatisticHistoryAsync(string type, int patientId, string dbName)
        {
            throw new NotImplementedException();
        }

        public async Task<Patients> CreatePatientsAsync(Patients entity, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(entity);
                FormatUtil.SetIsActive<Patients>(entity, true);
                FormatUtil.SetDateBaseEntity<Patients>(entity);
                entity.IsAlive = true;

                var newId = await _repository.Add(dbName, entity);
                entity.Id = newId;

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "CreatePatientsAsync", MethodType.Create, nameof(Patients));

                var species = await _unitOfWork.AnimalRepository.GetByName(dbName, entity.Species);
                if (species == null)
                {
                    //add species
                    var newSpecies = new Animals
                    {
                        Name = entity.Species,
                    };
                    FormatUtil.SetIsActive<Animals>(newSpecies, true);
                    FormatUtil.SetDateBaseEntity<Animals>(newSpecies);
                    var newSpeciesId = await _unitOfWork.AnimalRepository.Add(dbName, newSpecies);

                    //add event log
                    await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newSpeciesId, "CreatePatientsAsync", MethodType.Create, nameof(Animals));

                    //add breed
                    var newBreed = new Breeds
                    {
                        AnimalsId = newSpeciesId,
                        Name = entity.Breed,
                    };
                    FormatUtil.SetIsActive<Breeds>(newBreed, true);
                    FormatUtil.SetDateBaseEntity<Breeds>(newBreed);
                    var newBreedId = await _unitOfWork.BreedRepository.Add(dbName, newBreed);

                    //add event log
                    await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newBreedId, "CreatePatientsAsync", MethodType.Create, nameof(Breeds));
                }
                else
                {
                    var breed = await _unitOfWork.BreedRepository.GetByName(dbName, species.Id, entity.Breed);
                    if (breed == null)
                    {
                        //add breed
                        var newBreed = new Breeds
                        {
                            AnimalsId = species.Id,
                            Name = entity.Breed,
                        };
                        FormatUtil.SetIsActive<Breeds>(newBreed, true);
                        FormatUtil.SetDateBaseEntity<Breeds>(newBreed);
                        var newBreedId = await _unitOfWork.BreedRepository.Add(dbName, newBreed);

                        //add event log
                        await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newBreedId, "CreatePatientsAsync", MethodType.Create, nameof(Breeds));
                    }
                }

                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"PatientsService.CreatePatientsAsync";
                throw;
            }
        }
    }
}
