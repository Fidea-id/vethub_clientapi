using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;

namespace Application.Services.Implementations
{
    public class AppointmentService : GenericService<Appointments, AppointmentsRequest, Appointments, AppointmentsFilter>, IAppointmentService
    {
        public AppointmentService(IUnitOfWork unitOfWork, IGenericRepository<Appointments, AppointmentsFilter> repository)
        : base(unitOfWork, repository)
        { }

        public async Task<IEnumerable<AppointmentsDetailResponse>> GetDetailAppointmentList(string dbName)
        {
            var result = await _unitOfWork.AppointmentRepository.GetAllDetailList(dbName);
            return result;
        }

        public async Task<AppointmentsDetailResponse> GetDetailAppointment(int id, string dbName)
        {
            var result = await _unitOfWork.AppointmentRepository.GetAllDetail(id, dbName);
            return result;
        }

        public async Task<IEnumerable<AppointmentsStatus>> GetStatus(string dbName)
        {
            var data = await _unitOfWork.AppointmentRepository.GetAllStatus(dbName);
            return data;
        }
    }
}
