using Domain.Entities.Filters;
using Domain.Entities.Models;

namespace Domain.Interfaces
{
    public interface IPatientsRepository : IGenericRepository<Patients, PatientsFilter>
    {
    }
}
