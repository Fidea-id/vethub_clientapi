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
            await SetExpiredBooking(dbName);
            var result = await _unitOfWork.AppointmentRepository.GetAllDetailList(dbName, filter);
            return result;
        }
        public async Task<IEnumerable<AppointmentsDetailResponse>> GetDetailAppointmentListToday(string dbName)
        {
            await SetExpiredBooking(dbName);
            var result = await _unitOfWork.AppointmentRepository.GetAllDetailListToday(dbName);
            return result;
        }

        public async Task<AppointmentsDetailResponse> GetDetailAppointment(int id, string dbName)
        {
            await SetExpiredBooking(dbName);
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
            if (data == null)
                throw new Exception("Appointment not found");
            var staff = await _unitOfWork.ProfileRepository.GetByGlobalId(dbName, request.StaffId.Value);
            if (staff == null)
                throw new Exception("Staff not found");
            var statusId = request.StatusId.Value;
            if (statusId != data.StatusId + 1)
            {
                throw new Exception("Unallowed status change");
            }
            data.StatusId = statusId;
            FormatUtil.SetDateBaseEntity<Appointments>(data, true);
            await _repository.Update(dbName, data);
            var now = DateTime.Now;

            if (statusId == 3) //buat medical record
            {
                var getService = await _unitOfWork.ServicesRepository.GetById(dbName, data.ServiceId);
                var getLatestCode = await _unitOfWork.MedicalRecordsRepository.GetLatestCode(dbName);
                var newMedicalRecord = new MedicalRecords
                {
                    Code = FormatUtil.GenerateMedicalRecordCode(getLatestCode),
                    AppointmentId = data.Id,
                    IsActive = true,
                    StartDate = now,
                    EndDate = null,
                    PatientId = data.PatientsId,
                    PaymentStatus = "Unpaid",
                    StaffId = staff.Id,
                    Total = getService.Price
                };

                FormatUtil.SetDateBaseEntity<MedicalRecords>(newMedicalRecord);
                await _unitOfWork.MedicalRecordsRepository.Add(dbName, newMedicalRecord);
            }
            if (statusId == 5) //buat invoice
            {

            }

            // add appointment activity
            var newAppointment = new AppointmentsActivity()
            {
                AppointmentId = data.Id,
                CurrentDate = now,
                CurrentStatusId = statusId,
                StaffId = staff.Id,
                Note = request.Notes
            };
            FormatUtil.SetDateBaseEntity<AppointmentsActivity>(newAppointment);
            await _unitOfWork.AppointmentRepository.AddActivity(newAppointment, dbName);
        }

        private async Task SetExpiredBooking(string dbName)
        {
            var getExpiredBooking = await _unitOfWork.AppointmentRepository.WhereQuery(dbName, $"StatusId = 1 AND Date < CURRENT_DATE");
            foreach (var item in getExpiredBooking)
            {
                item.StatusId = 7; //expired booking
                FormatUtil.SetDateBaseEntity<Appointments>(item, true);
                await _unitOfWork.AppointmentRepository.Update(dbName, item);
            }
        }
    }
}
