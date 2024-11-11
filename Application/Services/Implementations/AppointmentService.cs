using Application.Services.Contracts;
using Domain.Entities;
using Domain.Entities.DTOs;
using Domain.Entities.Emails;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces;
using Domain.Interfaces.Clients;
using Domain.Utils;
using FluentEmail.Core.Models;
using Newtonsoft.Json;
using System.Web;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services.Implementations
{
    public class AppointmentService : GenericService<Appointments, AppointmentsRequest, Appointments, AppointmentsFilter>, IAppointmentService
    {
        private readonly IEmailSender _emailsender;
        public AppointmentService(IUnitOfWork unitOfWork, IGenericRepository<Appointments, AppointmentsFilter> repository, ICurrentUserService currentUser, IEmailSender emailsender)
        : base(unitOfWork, repository, currentUser)
        {
            _emailsender = emailsender;
        }

        public async Task SendInvoiceEmail(string dbName, int appointmentId)
        {
            //send email to owner pet
            var medicalRecord = await _unitOfWork.MedicalRecordsRepository.GetByAppointmentId(dbName, appointmentId);
            var owner = await _unitOfWork.OwnersRepository.ReadByPatientIdAsync(medicalRecord.PatientId, dbName);
            if (owner != null)
            {
                if (!string.IsNullOrEmpty(owner.Email))
                {
                    var clinics = await _unitOfWork.ClinicsRepository.GetAll(dbName);
                    var clinic = clinics.FirstOrDefault();

                    var baseurl = "https://app.vethub.id/Appointment/PubInvoice";

                    var code = HttpUtility.UrlEncode(EncryptionHelper.EncryptString(dbName));
                    var urlId = HttpUtility.UrlEncode(EncryptionHelper.EncryptString(medicalRecord.Id.ToString()));
                    var queryUrl = $"?e={code}&d={urlId}";
                    var data = new EmailSenderData
                    {
                        Subject = "Appointment Invoice",
                        To = owner.Email,
                        CC = new List<Address>()
                        {
                        },
                        EmailData = new InvoiceEmailData()
                        {
                            ClinicName = clinic.Name,
                            OwnerName = owner.Name,
                            Url = baseurl + queryUrl,
                            Date = DateTime.Now
                        }
                    };

                    await _emailsender.Send(data);
                }
            }
            else
            {
            }
        }

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

        public async Task<InvoiceResponse> GetDetailMedicalInvoice(int medicalId, string dbName)
        {
            var medicalRecords = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, medicalId);
            var appointments = await _unitOfWork.AppointmentRepository.GetById(dbName, medicalRecords.AppointmentId);
            var services = await _unitOfWork.ServicesRepository.GetById(dbName, appointments.ServiceId);
            var staff = await _unitOfWork.ProfileRepository.GetById(dbName, medicalRecords.StaffId);
            var patient = await _unitOfWork.PatientsRepository.GetById(dbName, medicalRecords.PatientId);
            var owner = await _unitOfWork.OwnersRepository.GetById(dbName, patient.OwnersId);
            var notes = await _unitOfWork.MedicalRecordsNotesRepository.GetByMedicalRecordId(dbName, medicalRecords.Id);
            var diagnoses = await _unitOfWork.MedicalRecordsDiagnosesRepository.GetByMedicalRecordId(dbName, medicalRecords.Id);
            var presciptions = await _unitOfWork.MedicalRecordsPrescriptionsRepository.GetByMedicalRecordId(dbName, medicalRecords.Id);
            var lastPayments = await _unitOfWork.OrdersPaymentRepository.GetPaidByOrderId(dbName, medicalRecords.Id, "MedicalRecord");
            var totalLastPayment = lastPayments.Sum(x => x.Total);
            var opnamePatients = await _unitOfWork.OpnamePatientsRepository.GetByMedId(dbName, medicalRecords.Id);
            var dataOpnamePatients = opnamePatients.Data.FirstOrDefault();
            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetAll(dbName);

            var opnameDetail = new OpnameDetailResponse();
            if (dataOpnamePatients != null)
            {
                var dataOpname = await _unitOfWork.OpnamesRepository.GetById(dbName, dataOpnamePatients.OpnameId);
                var _opnameDetail = new OpnameDetailResponse()
                {
                    OpnameName = dataOpname.Name,
                    StartTime = dataOpnamePatients.StartTime,
                    EstimatedDays = dataOpnamePatients.EstimateDays,
                    EndTime = dataOpnamePatients.EndTime,
                    OpnameId = dataOpnamePatients.OpnameId,
                    OpnamePatientsId = dataOpnamePatients.Id,
                    Price = dataOpnamePatients.Price,
                    Status = dataOpnamePatients.Status,
                    TotalPrice = dataOpnamePatients.TotalPrice
                };

                opnameDetail = _opnameDetail;
            }

            var medicalDetail = new MedicalRecordsDetailResponse
            {
                Id = medicalRecords.Id,
                Code = medicalRecords.Code,
                Appointments = appointments,
                Patients = patient,
                Services = services,
                Owners = owner,
                Staff = staff,
                DiscountMethod = medicalRecords.DiscountMethod,
                DiscountValue = medicalRecords.DiscountValue,
                DiscountTotal = medicalRecords.DiscountTotal,
                TotalDiscounted = medicalRecords.TotalDiscounted,
                StartDate = medicalRecords.StartDate,
                EndDate = medicalRecords.EndDate.Value,
                TotalPrice = medicalRecords.Total,
                TotalPaid = totalLastPayment,
                Prescriptions = presciptions,
                Diagnoses = diagnoses,
                Notes = notes,
                OpnameDetail = opnameDetail
            };

            var clinicData = await _unitOfWork.ClinicsRepository.GetAll(dbName);
            var response = new InvoiceResponse
            {
                Detail = medicalDetail,
                ClinicData = clinicData.FirstOrDefault(),
                PaymentData = lastPayments,
                PaymentMethodData = paymentMethod
            };

            return response;
        }

        public async Task ChangeAppointmentStatus(AppointmentsRequestChangeStatus request, string dbName)
        {
            var currentUserId = await _currentUser.UserId;
            var data = await _repository.GetById(dbName, request.Id.Value);
            if (data == null)
                throw new Exception("Appointment not found");
            var staff = await _unitOfWork.ProfileRepository.GetByGlobalId(dbName, request.StaffId.Value);
            if (staff == null)
                throw new Exception("Staff not found");
            var statusId = request.StatusId.Value;
            data.StatusId = statusId;
            FormatUtil.SetDateBaseEntity<Appointments>(data, true);
            await _repository.Update(dbName, data);

            //add event log
            await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, data.Id, "ChangeAppointmentStatus", MethodType.Update, nameof(Appointments),"Update Status to: " + statusId);

            var now = DateTime.Now;

            if (statusId == 3) //buat medical record
            {
                double initPrice = 0;
                if(data.ServiceId != null && data.ServiceId != 0)
                {
                    var getService = await _unitOfWork.ServicesRepository.GetById(dbName, data.ServiceId);
                    initPrice = getService.Price;
                }
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
                    Total = initPrice
                };

                FormatUtil.SetDateBaseEntity<MedicalRecords>(newMedicalRecord);
                var newId = await _unitOfWork.MedicalRecordsRepository.Add(dbName, newMedicalRecord);

                //add event log
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "ChangeAppointmentStatus", MethodType.Create, nameof(MedicalRecords));
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, item.Id, "SetExpiredBooking", MethodType.Update, nameof(Appointments), "Update Status to: " + item.StatusId);
            }
        }

        public async Task<DataResultDTO<AppointmentMedicalDetailResponse>> GetDetailAppointmentMedicalList(AppointmentDetailFilter filter, string dbName)
        {
            var result = await _unitOfWork.AppointmentRepository.GetAllDetailMedicalList(dbName, filter);
            return result;
        }

        public async Task<DataResultDTO<AppointmentsType>> ReadAllAppointmentsTypeAsync(string dbName)
        {
            try
            {
                var filePath = $"{Directory.GetCurrentDirectory()}/wwwroot/DataInsert/initData.json";
                var existingData = await _unitOfWork.AppointmentsTypeRepository.GetAll(dbName);
                if(existingData.Count() > 0)
                {
                    var results = new DataResultDTO<AppointmentsType>()
                    {
                        Data = existingData,
                        TotalData = existingData.Count()
                    };
                    return results;
                }

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var deserializedObjects = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (deserializedObjects != null && deserializedObjects.ContainsKey("AppointmentsType"))
                    {
                        var appointmentsTypeJson = deserializedObjects["AppointmentsType"].ToString();
                        var dataTemplate = JsonConvert.DeserializeObject<IEnumerable<AppointmentsType>>(appointmentsTypeJson);

                        foreach (var item in dataTemplate)
                        {
                            var existingConfig = existingData.FirstOrDefault(x => x.Name == item.Name);
                            if (existingConfig == null)
                            {
                                var newItem = new AppointmentsType
                                {
                                    Name = item.Name,
                                };
                                FormatUtil.SetIsActive<AppointmentsType>(newItem, true);
                                FormatUtil.SetDateBaseEntity<AppointmentsType>(newItem);

                                await _unitOfWork.AppointmentsTypeRepository.Add(dbName, newItem);
                            }
                        }
                    }
                }

                var result = new DataResultDTO<AppointmentsType>()
                {
                    Data = existingData,
                    TotalData = existingData.Count()
                };
                return result;
            }
            catch (Exception ex)
            {
                ex.Source = $"AppointmentService.ReadAllAppointmentsTypeAsync";
                throw;
            }
        }

        public async Task<AppointmentsType> UpdateAppointmentsTypeAsync(string dbName, int id, AppointmentsType request)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                FormatUtil.SetDateBaseEntity<AppointmentsType>(request, true);

                AppointmentsType checkedEntity = await _unitOfWork.AppointmentsTypeRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<AppointmentsType, AppointmentsType>(request, checkedEntity);
                FormatUtil.SetIsActive<AppointmentsType>(checkedEntity, true);
                await _unitOfWork.AppointmentsTypeRepository.Update(dbName, checkedEntity);

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "Appointments-UpdateAppointmentsTypeAsync", MethodType.Update, "AppointmentsType");
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AppointmentService.UpdateAppointmentsTypeAsync";
                throw;
            }
        }

        public async Task<AppointmentsType> AddAppointmentsTypeAsync(string dbName, AppointmentsType request)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                FormatUtil.SetIsActive<AppointmentsType>(request, true);
                FormatUtil.SetDateBaseEntity<AppointmentsType>(request);

                var newId = await _unitOfWork.AppointmentsTypeRepository.Add(dbName, request);
                request.Id = newId;

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "Appointments-AddAppointmentsTypeAsync", MethodType.Create, "AppointmentsType");
                return request;
            }
            catch (Exception ex)
            {
                ex.Source = $"AppointmentService.AddAppointmentsTypeAsync";
                throw;
            }
        }

        public async Task DeleteAppointmentsTypeAsync(string dbName, int id)
        {
            try
            {
                //get entity
                var entity = await _unitOfWork.AppointmentsTypeRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _unitOfWork.AppointmentsTypeRepository.Remove(dbName, id);
                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "Appointments-DeleteAppointmentsTypeAsync", MethodType.Delete, "AppointmentsType");
            }
            catch (Exception ex)
            {
                ex.Source = $"AppointmentService.DeleteAppointmentsTypeAsync";
                throw;
            }
        }
    }
}
