using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests;

namespace Application.Services.Contracts
{
    public interface IClinicsService : IGenericService<Clinics, ClinicsRequest, Clinics, ClinicsFilter>
    {
    }
}
