using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IMedicalRecordService : IGenericService<MedicalRecords, MedicalRecordsRequest, MedicalRecordsResponse, MedicalRecordsFilter>
    {
        Task<MedicalRecordsDetailResponse> GetDetailMedicalRecords(int id, string dbName);
        Task<DataResultDTO<BookingHistoryResponse>> GetBookingHistoryByOwner(int ownerId, string dbName); //seperti detail medical records, tapi lebih ringkas
        Task<DataResultDTO<BookingHistoryResponse>> GetBookingHistoryByPatient(int patientId, string dbName); //seperti detail medical records, tapi lebih ringkas
        Task<OrdersPaymentResponse> AddOrdersPaymentAsync(OrdersPaymentRequest request, string dbName);
        Task<MedicalRecordsDetailResponse> PostAllMedicalRecords(MedicalRecordsDetailRequest request, string dbName);
        Task<MedicalRecordsNotesResponse> PostMedicalRecordsNotes(MedicalRecordsNotesRequest request, string email, string dbName);
        Task<MedicalRecordsNotesResponse> PutMedicalRecordsNotes(int id, MedicalRecordsNotesRequest request, string email, string dbName);
        Task<MedicalDocsRequirementResponse> GetMedicalRecordRequirement(int medicalRecordId, string dbName);
        Task<IEnumerable<MedicalRecordsNotes>> GetMedicalRecordsNotes(int id, string dbName);
        Task DeleteMedicalRecordsNotes(int id, string dbName);
    }
}
