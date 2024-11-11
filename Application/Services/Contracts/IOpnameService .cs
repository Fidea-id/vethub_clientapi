using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IOpnameService : IGenericService<Opnames, OpnamesRequest, Opnames, OpnamesFilter>
    {
        //opname patients
        Task<OpnamePatients> CreateOpnamePatientsAsync(OpnamePatientsRequest entity, string dbName);
        Task<DataResultDTO<OpnamePatients>> ReadOpnamePatientsAllAsync(OpnamePatientsFilter filter, string dbName);
        Task<DataResultDTO<OpnamePatientsDetailResponse>> ReadOpnamePatientsDetailAsync(OpnamePatientsFilter filter, string dbName);
        Task<OpnamePatients> ReadOpnamePatientsByIdAsync(int id, string dbName);
        Task<DataResultDTO<OpnamePatients>> ReadOpnamePatientsByMedIdAsync(int id, string dbName);
        Task<DataResultDTO<OpnamePatients>> ReadOpnamePatientsByOpnameIdAsync(int id, string dbName);
        Task<OpnamePatients> UpdateOpnamePatientsAsync(int id, OpnamePatientsRequest entity, string dbName);
        Task DeleteOpnamePatientsAsync(int id, string dbName);

        //detail opname patients list
    }
}
