using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests;
using Domain.Interfaces.Clients;

namespace Application.Services.Implementations
{
    public class ClinicsService : GenericService<Clinics, ClinicsRequest, Clinics, ClinicsFilter>, IClinicsService
    {
        public ClinicsService(IUnitOfWork unitOfWork, IGenericRepository<Clinics, ClinicsFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}

