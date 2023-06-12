using Domain.Entities.Filters;
using Domain.Entities.Models;
using Domain.Entities.Requests;

namespace Application.Services.Contracts
{
    public interface IPatientsService : IGenericService<Patients, PatientsRequest, Patients, PatientsFilter>
    {
    }
}
