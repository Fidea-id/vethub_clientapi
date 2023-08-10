using Application.Services.Contracts;
using Application.Utils;
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
        public PatientsService(IUnitOfWork unitOfWork, IGenericRepository<Patients, PatientsFilter> repository)
        : base(unitOfWork, repository)
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
    }
}
