using Application.Services.Contracts;
using Domain.Entities.Filters;
using Domain.Entities.Models;
using Domain.Entities.Requests;
using Domain.Interfaces;

namespace Application.Services.Implementations
{
    public class PatientsService : GenericService<Patients, PatientsRequest, Patients, PatientsFilter>, IPatientsService
    {
        public PatientsService(IUnitOfWork unitOfWork, IGenericRepository<Patients, PatientsFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}
