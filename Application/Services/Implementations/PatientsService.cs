using Application.Services.Contracts;
using Domain.Entities.Models;
using Domain.Interfaces;

namespace Application.Services.Implementations
{
    public class PatientsService : GenericService<Patients, Patients, Patients, PatientsFilter>, IPatientsService
    {
        public PatientsService(IUnitOfWork unitOfWork, IGenericRepository<Patients, PatientsFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}
