using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Contracts
{
    public interface IMedicalRecordService : IGenericService<MedicalRecords, MedicalRecordsRequest, MedicalRecordsResponse, MedicalRecordsFilter>
    {
        Task<MedicalRecordsDetailResponse> PostAllMedicalRecords(MedicalRecordsDetailRequest request, string dbName);
        Task<MedicalRecordsNotesResponse> PostMedicalRecordsNotes(MedicalRecordsNotesRequest request, string email, string dbName);
        Task<MedicalDocsRequirementResponse> GetMedicalRecordRequirement(int medicalRecordId, string dbName);
        Task<IEnumerable<MedicalRecordsNotes>> GetMedicalRecordsNotes(int id, string dbName);
        Task DeleteMedicalRecordsNotes(int id, string dbName);
    }
}
