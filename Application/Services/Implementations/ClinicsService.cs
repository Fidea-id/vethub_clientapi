using Application.Services.Contracts;
using Domain.Entities.Filters;
using Domain.Entities.Models;
using Domain.Entities.Requests;
using Domain.Interfaces;

namespace Application.Services.Implementations
{
    public class ClinicsService : GenericService<Domain.Entities.Models.Clinics, ClinicsRequest, Domain.Entities.Models.Clinics, ClinicsFilter>, IClinicsService
    {
        public ClinicsService(IUnitOfWork unitOfWork, IGenericRepository<Domain.Entities.Models.Clinics, ClinicsFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}

