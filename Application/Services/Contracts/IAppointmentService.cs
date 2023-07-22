using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IAppointmentService : IGenericService<Appointments, AppointmentsRequest, Appointments, AppointmentsFilter>
    {
        Task<IEnumerable<AppointmentsStatus>> GetStatus(string dbName);
        Task<IEnumerable<AppointmentsDetailResponse>> GetDetailAppointmentList(string dbName);
        Task<AppointmentsDetailResponse> GetDetailAppointment(int id, string dbName);
    }
}
