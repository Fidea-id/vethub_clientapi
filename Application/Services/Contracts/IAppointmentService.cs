using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IAppointmentService : IGenericService<Appointments, AppointmentsRequest, Appointments, AppointmentsFilter>
    {
        Task SendInvoiceEmail(string dbName, int appointmentId);
        Task<IEnumerable<AppointmentsStatus>> GetStatus(string dbName);
        Task<InvoiceResponse> GetDetailMedicalInvoice(int medicalId, string dbName);
        Task<DataResultDTO<AppointmentsDetailResponse>> GetDetailAppointmentList(AppointmentDetailFilter filter, string dbName);
        Task<DataResultDTO<AppointmentMedicalDetailResponse>> GetDetailAppointmentMedicalList(AppointmentDetailFilter filter, string dbName);
        Task<IEnumerable<AppointmentsDetailResponse>> GetDetailAppointmentListToday(string dbName);
        Task<AppointmentsDetailResponse> GetDetailAppointment(int id, string dbName);

        Task ChangeAppointmentStatus(AppointmentsRequestChangeStatus request, string dbName);

        Task<DataResultDTO<AppointmentsType>> ReadAllAppointmentsTypeAsync(string dbName);
        Task<AppointmentsType> UpdateAppointmentsTypeAsync(string dbName, int id, AppointmentsType request);
        Task<AppointmentsType> AddAppointmentsTypeAsync(string dbName, AppointmentsType request);
        Task DeleteAppointmentsTypeAsync(string dbName, int id);
    }
}
