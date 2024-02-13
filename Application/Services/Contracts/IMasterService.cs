using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Masters;

namespace Application.Services.Contracts
{
    public interface IMasterService
    {
        Task GenerateTables(string dbName, int? version = null);
        Task GenerateTableField(string dbName);
        Task<Clinics> CreateClinicsProfileAsync(RegisterClinicProfileRequest data, string dbName);
    }
}
