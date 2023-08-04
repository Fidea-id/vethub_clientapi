using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IPatientsService : IGenericService<Patients, PatientsRequest, Patients, PatientsFilter>
    {
        Task<DataResultDTO<PatientsListResponse>> ReadPatientsList(PatientsFilter filter, string dbName);
        Task<IEnumerable<Patients>> ReadByOwnerIdAsync(int id, string dbName);


        Task<PatientsStatistic> AddPatientStatistic(PatientsStatisticRequest request, string dbName);
        Task<IEnumerable<PatientsStatisticResponse>> ReadPatientsStatisticAsync(int patientId, string dbName);
        Task<IEnumerable<PatientsStatisticHistoryResponse>> ReadPatientsStatisticHistoryAsync(string type, int patientId, string dbName);
    }
}
