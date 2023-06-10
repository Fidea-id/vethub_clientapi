using Domain.Entities.Models;

namespace Application.Services.Contracts
{
    public interface IPatientsService : IGenericService<Patients, Patients, Patients, PatientsFilter>
    {
    }
}
