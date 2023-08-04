using Application.Services.Contracts;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;

namespace Application.Services.Implementations
{
    public class AppointmentService : GenericService<Appointments, AppointmentsRequest, Appointments, AppointmentsFilter>, IAppointmentService
    {
        public AppointmentService(IUnitOfWork unitOfWork, IGenericRepository<Appointments, AppointmentsFilter> repository)
        : base(unitOfWork, repository)
        { }

        public async Task<DataResultDTO<AppointmentsDetailResponse>> GetDetailAppointmentList(AppointmentDetailFilter filter, string dbName)
        {
            var result = await _unitOfWork.AppointmentRepository.GetAllDetailList(dbName, filter);
            return result;
        }
        public async Task<IEnumerable<AppointmentsDetailResponse>> GetDetailAppointmentListToday(string dbName)
        {
            var result = await _unitOfWork.AppointmentRepository.GetAllDetailListToday(dbName);
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

        public async Task ChangeAppointmentStatus(AppointmentsRequestChangeStatus request, string dbName)
        {
            var data = await _repository.GetById(dbName, request.Id.Value);
            var statusId = request.StatusId.Value;
            if (statusId != data.StatusId + 1)
            {
                throw new Exception("Unallowed status change");
            }
            data.StatusId = statusId;
            FormatUtil.SetDateBaseEntity<Appointments>(data, true);
            await _repository.Update(dbName, data);

            // add appointment activity
            var newAppointment = new AppointmentsActivity()
            {
                AppointmentId = data.Id,
                CurrentDate = DateTime.Now,
                CurrentStatusId = statusId,
                StaffId = request.StaffId.Value,
                Note = request.Notes
            };

            await _unitOfWork.AppointmentRepository.AddActivity(newAppointment, dbName);
        }
    }
}
