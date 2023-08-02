﻿using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IAppointmentService : IGenericService<Appointments, AppointmentsRequest, Appointments, AppointmentsFilter>
    {
        Task<IEnumerable<AppointmentsStatus>> GetStatus(string dbName);
        Task<IEnumerable<AppointmentsDetailResponse>> GetDetailAppointmentList(AppointmentDetailFilter filter, string dbName);
        Task<IEnumerable<AppointmentsDetailResponse>> GetDetailAppointmentListToday(string dbName);
        Task<AppointmentsDetailResponse> GetDetailAppointment(int id, string dbName);

        Task ChangeAppointmentStatus(AppointmentsRequestChangeStatus request, string dbName);
    }
}
