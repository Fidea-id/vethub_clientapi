using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IPatientsService : IGenericService<Patients, PatientsRequest, Patients, PatientsFilter>
    {
        Task<IEnumerable<PatientsListResponse>> ReadPatientsList(PatientsFilter filter, string dbName);
    }
}
