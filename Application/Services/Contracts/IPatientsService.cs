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
    }
}
