using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities;
using Domain.Entities.DTOs;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services.Implementations
{
    public class MedicalRecordService : GenericService<MedicalRecords, MedicalRecordsRequest, MedicalRecordsResponse, MedicalRecordsFilter>, IMedicalRecordService
    {
        private readonly ILogger<MedicalRecordService> _logger;
        public MedicalRecordService(IUnitOfWork unitOfWork, IGenericRepository<MedicalRecords, MedicalRecordsFilter> repository,
            ILoggerFactory loggerFactory, ICurrentUserService currentUser)
        : base(unitOfWork, repository, currentUser)
        {
            _logger = loggerFactory.CreateLogger<MedicalRecordService>();
        }

        public async Task<DataResultDTO<MedicalRecordsHistoryResponse>> GetMedicalRecordHistory(int medId, string dbName)
        {
            var medicalRecords = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, medId);
            if (medicalRecords == null) throw new Exception("Medical records not found");

            var historical = await _unitOfWork.EventLogRepository.GetEventLogByObjectId(dbName, medId, "MedicalRecordsDetailResponse", "PostAllMedicalRecords");
            var result = new List<MedicalRecordsHistoryResponse>();
            foreach(var item in historical.Data)
            {
                var convMedHistory = JsonConvert.DeserializeObject<MedicalRecordsDetailResponse>(item.Detail);
                var newMedHistory = new MedicalRecordsHistoryResponse()
                {
                    Date = item.CreatedAt,
                    Detail = convMedHistory
                };
                result.Add(newMedHistory);
            }
            if (result.Count > 0)
            {
                result.RemoveAt(result.Count - 1);
            }
            return new DataResultDTO<MedicalRecordsHistoryResponse> { Data = result, TotalData = result.Count() };
        }

        public async Task<MedicalRecordsDetailResponse> GetDetailMedicalRecords(int id, string dbName, string flag = null)
        {
            var medicalRecords = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, id);
            var appointments = await _unitOfWork.AppointmentRepository.GetById(dbName, medicalRecords.AppointmentId);
            var services = await _unitOfWork.ServicesRepository.GetById(dbName, appointments.ServiceId);
            var staff = await _unitOfWork.ProfileRepository.GetById(dbName, medicalRecords.StaffId);
            var patient = await _unitOfWork.PatientsRepository.GetById(dbName, medicalRecords.PatientId);
            var owner = await _unitOfWork.OwnersRepository.GetById(dbName, patient.OwnersId);
            IEnumerable<MedicalRecordsNotes> notes = null;
            if (flag != "no_notes")
            {
                notes = await _unitOfWork.MedicalRecordsNotesRepository.GetByMedicalRecordId(dbName, medicalRecords.Id);
            }
            var diagnoses = await _unitOfWork.MedicalRecordsDiagnosesRepository.GetByMedicalRecordId(dbName, medicalRecords.Id);
            var presciptions = await _unitOfWork.MedicalRecordsPrescriptionsRepository.GetByMedicalRecordId(dbName, medicalRecords.Id);
            var lastPayments = await _unitOfWork.OrdersPaymentRepository.GetPaidByOrderId(dbName, medicalRecords.Id, "MedicalRecord");
            var totalLastPayment = lastPayments.Sum(x => x.Total);
            string statusPayment = "Paid";
            if (lastPayments.Count() < 1)
            {
                statusPayment = "Unpaid";
            }
            else if (totalLastPayment < medicalRecords.Total)
            {
                statusPayment = "Paid Less";
            }

            var opnamePatients = await _unitOfWork.OpnamePatientsRepository.GetByMedId(dbName, medicalRecords.Id);
            var dataOpnamePatients = opnamePatients.Data.FirstOrDefault();

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

            var response = new MedicalRecordsDetailResponse
            {
                Id = medicalRecords.Id,
                Code = medicalRecords.Code,
                Appointments = appointments,
                Patients = patient,
                Services = services,
                Owners = owner,
                Staff = staff,
                StartDate = medicalRecords.StartDate,
                DiscountMethod = medicalRecords.DiscountMethod,
                DiscountValue = medicalRecords.DiscountValue,
                DiscountTotal = medicalRecords.DiscountTotal,
                TotalDiscounted = medicalRecords.TotalDiscounted,
                EndDate = medicalRecords.EndDate.Value,
                TotalPrice = medicalRecords.Total,
                TotalPaid = totalLastPayment,
                Prescriptions = presciptions,
                Diagnoses = diagnoses,
                StatusPayment = statusPayment,
                Notes = notes,
                OpnameDetail = opnameDetail
            };
            return response;
        }
        public async Task<DataResultDTO<BookingHistoryResponse>> GetBookingHistoryByOwner(int ownerId, string dbName)
        {
            var data = await _unitOfWork.AppointmentRepository.GetBookingHistoryOwner(dbName, ownerId);
            if (data.Count() > 0)
            {
                foreach (var item in data)
                {
                    var diagnoses = await _unitOfWork.MedicalRecordsDiagnosesRepository.GetByMedicalRecordId(dbName, item.MedicalRecordsId);
                    item.Diagnoses = diagnoses;
                }
            }
            return new DataResultDTO<BookingHistoryResponse> { Data = data, TotalData = data.Count() };
        }

        public async Task<DataResultDTO<BookingHistoryResponse>> GetBookingHistoryByPatient(int patientId, string dbName)
        {
            var data = await _unitOfWork.AppointmentRepository.GetBookingHistoryPatient(dbName, patientId);
            if (data.Count() > 0)
            {
                foreach (var item in data)
                {
                    var diagnoses = await _unitOfWork.MedicalRecordsDiagnosesRepository.GetByMedicalRecordId(dbName, item.MedicalRecordsId);
                    item.Diagnoses = diagnoses;
                }
            }
            return new DataResultDTO<BookingHistoryResponse> { Data = data, TotalData = data.Count() };
        }

        public async Task<IEnumerable<MedicalRecordsPrescriptions>> EditMedicalRecordPrescription(int medicalRecordId, IEnumerable<MedicalRecordsPrescriptionsRequest> request, string dbName)
        {
            if (request.Count() > 0)
            {
                var currentUserId = await _currentUser.UserId;
                var medicalRecords = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, medicalRecordId);
                var medicalRecordsPrescriptions = await _unitOfWork.MedicalRecordsPrescriptionsRepository.GetByMedicalRecordId(dbName, medicalRecordId);
                double currentTotal = 0;
                if (medicalRecordsPrescriptions.Count() > 0)
                {
                    foreach (var pItem in medicalRecordsPrescriptions)
                    {
                        await _unitOfWork.MedicalRecordsPrescriptionsRepository.Remove(dbName, pItem.Id);
                    }
                    currentTotal = medicalRecordsPrescriptions.Sum(x => x.Total);
                }
                var totalNow = medicalRecords.Total - currentTotal;
                var prescriptionData = new List<MedicalRecordsPrescriptions>();

                // add data prescription
                foreach (var pItem in request)
                {
                    //trim all string
                    FormatUtil.TrimObjectProperties(pItem);
                    var entity = Mapping.Mapper.Map<MedicalRecordsPrescriptions>(pItem);
                    FormatUtil.SetIsActive<MedicalRecordsPrescriptions>(entity, true);
                    FormatUtil.SetDateBaseEntity<MedicalRecordsPrescriptions>(entity);
                    entity.MedicalRecordsId = medicalRecords.Id;
                    var newId = await _unitOfWork.MedicalRecordsPrescriptionsRepository.Add(dbName, entity);
                    entity.Id = newId;
                    prescriptionData.Add(entity);

                    totalNow = totalNow + pItem.Total;
                }

                FormatUtil.SetDateBaseEntity<MedicalRecords>(medicalRecords);
                medicalRecords.Total = totalNow;
                await _unitOfWork.MedicalRecordsRepository.Update(dbName, medicalRecords);

                //add event log
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, medicalRecords.Id, "EditMedicalRecordPrescription", MethodType.Update, nameof(MedicalRecords), JsonConvert.SerializeObject(request));

                return prescriptionData;
            }
            return default(List<MedicalRecordsPrescriptions>);
        }

        //public async Task<MedicalRecordsDetailResponse> PostAllMedicalRecords(MedicalRecordsDetailRequest request, string dbName)
        //{
        //    var currentUserId = await _currentUser.UserId;
        //    var medicalRecords = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, request.MedicalRecordsId);
        //    var appointment = await _unitOfWork.AppointmentRepository.GetById(dbName, medicalRecords.AppointmentId);
        //    var staff = await _unitOfWork.ProfileRepository.GetById(dbName, medicalRecords.StaffId);
        //    //update appointment status to pharmacy
        //    if (appointment.StatusId == 3)// check if the appointment is not edited
        //    {
        //        var currentStatus = 4; //update status to Pharmachy
        //        appointment.StatusId = currentStatus;
        //        FormatUtil.SetDateBaseEntity<MedicalRecords>(medicalRecords, true);
        //        await _unitOfWork.AppointmentRepository.Update(dbName, appointment);

        //        // add appointment activity
        //        var newAppointment = new AppointmentsActivity()
        //        {
        //            AppointmentId = appointment.Id,
        //            CurrentDate = DateTime.Now,
        //            CurrentStatusId = currentStatus,
        //            StaffId = medicalRecords.StaffId,
        //            Note = ""
        //        };
        //        FormatUtil.SetDateBaseEntity<AppointmentsActivity>(newAppointment);
        //        await _unitOfWork.AppointmentRepository.AddActivity(newAppointment, dbName);

        //        //add event log
        //        await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, appointment.Id, "PostAllMedicalRecords", MethodType.Update, nameof(Appointments), "Update status to : " + currentStatus);
        //    }

        //    var prescriptionData = new List<MedicalRecordsPrescriptions>();
        //    var diagnoseData = new List<MedicalRecordsDiagnoses>();

        //    // update data medical records
        //    medicalRecords.EndDate = DateTime.Now;
        //    if (request.Prescriptions.Count() > 0)
        //    {
        //        var currentTotal = medicalRecords.Total;
        //        if (appointment.StatusId != 3)// check if the appointment is edited
        //        {
        //            var currentPrescriptions = await _unitOfWork.MedicalRecordsPrescriptionsRepository.GetByMedicalRecordId(dbName, request.MedicalRecordsId);
        //            var currentPresTotal = currentPrescriptions.Sum(x => x.Total);
        //            //kurangi nilai total medical records saat ini dengan total prescription
        //            currentTotal = medicalRecords.Total - currentPresTotal;

        //            //delete current presc
        //            await _unitOfWork.MedicalRecordsPrescriptionsRepository.RemoveRange(dbName, currentPrescriptions);
        //        }

        //        var totalPrescription = request.Prescriptions.Sum(x => x.Total);
        //        medicalRecords.Total = currentTotal + totalPrescription;

        //        // add data prescription
        //        foreach (var pItem in request.Prescriptions)
        //        {
        //            //trim all string
        //            FormatUtil.TrimObjectProperties(pItem);
        //            var entity = Mapping.Mapper.Map<MedicalRecordsPrescriptions>(pItem);
        //            FormatUtil.SetIsActive<MedicalRecordsPrescriptions>(entity, true);
        //            FormatUtil.SetDateBaseEntity<MedicalRecordsPrescriptions>(entity);
        //            entity.MedicalRecordsId = medicalRecords.Id;
        //            var newId = await _unitOfWork.MedicalRecordsPrescriptionsRepository.Add(dbName, entity);
        //            entity.Id = newId;
        //            prescriptionData.Add(entity);
        //        }
        //    }
        //    FormatUtil.SetDateBaseEntity<MedicalRecords>(medicalRecords);
        //    await _unitOfWork.MedicalRecordsRepository.Update(dbName, medicalRecords);

        //    // add data notes
        //    if (request.Notes != null)
        //    {
        //        if (appointment.StatusId != 3)// check if the appointment is edited
        //        {
        //            var currentNotes = await _unitOfWork.MedicalRecordsNotesRepository.GetByMedicalRecordId(dbName, request.MedicalRecordsId);
        //            //delete current notes
        //            await _unitOfWork.MedicalRecordsNotesRepository.RemoveRange(dbName, currentNotes);
        //        }

        //        //trim all string
        //        FormatUtil.TrimObjectProperties(request.Notes);
        //        var entity = Mapping.Mapper.Map<MedicalRecordsNotes>(request.Notes);
        //        FormatUtil.SetIsActive<MedicalRecordsNotes>(entity, true);
        //        FormatUtil.SetDateBaseEntity<MedicalRecordsNotes>(entity);
        //        entity.MedicalRecordsId = medicalRecords.Id;
        //        entity.StaffId = medicalRecords.StaffId;
        //        var newId = await _unitOfWork.MedicalRecordsNotesRepository.Add(dbName, entity);
        //        entity.Id = newId;
        //    }

        //    var notes = await _unitOfWork.MedicalRecordsNotesRepository.GetByMedicalRecordId(dbName, medicalRecords.Id);

        //    if (appointment.StatusId != 3)// check if the appointment is edited
        //    {
        //        var currentDiagnoses = await _unitOfWork.MedicalRecordsDiagnosesRepository.GetByMedicalRecordId(dbName, request.MedicalRecordsId);
        //        //delete current diagnoses
        //        await _unitOfWork.MedicalRecordsDiagnosesRepository.RemoveRange(dbName, currentDiagnoses);
        //    }

        //    // add data diagnoses
        //    foreach (var dItem in request.Diagnoses)
        //    {
        //        //trim all string
        //        FormatUtil.TrimObjectProperties(dItem);
        //        var entity = Mapping.Mapper.Map<MedicalRecordsDiagnoses>(dItem);
        //        FormatUtil.SetIsActive<MedicalRecordsDiagnoses>(entity, true);
        //        FormatUtil.SetDateBaseEntity<MedicalRecordsDiagnoses>(entity);
        //        entity.MedicalRecordsId = medicalRecords.Id;
        //        var newId = await _unitOfWork.MedicalRecordsDiagnosesRepository.Add(dbName, entity);
        //        entity.Id = newId;
        //        diagnoseData.Add(entity);

        //        //check new diagnose
        //        var existDiagnose = await _unitOfWork.DiagnoseRepository.GetByFilter(dbName, new NameBaseEntityFilter() { Name = dItem.Diagnose });
        //        if (existDiagnose.TotalData < 1)
        //        {
        //            var newDiagnose = new Diagnoses()
        //            {
        //                Name = dItem.Diagnose,
        //            };
        //            _logger.LogInformation("Try add new diagnose : " + JsonConvert.SerializeObject(newDiagnose));
        //            FormatUtil.SetIsActive<Diagnoses>(newDiagnose, true);
        //            FormatUtil.SetDateBaseEntity<Diagnoses>(newDiagnose);

        //            var newDiagnoseId = await _unitOfWork.DiagnoseRepository.Add(dbName, newDiagnose);
        //            newDiagnose.Id = newDiagnoseId;
        //        }
        //    }

        //    var response = new MedicalRecordsDetailResponse
        //    {
        //        Id = medicalRecords.Id,
        //        Code = medicalRecords.Code,
        //        Staff = staff,
        //        Notes = notes,
        //        StartDate = medicalRecords.StartDate,
        //        EndDate = medicalRecords.EndDate.Value,
        //        TotalPrice = medicalRecords.Total,
        //        Prescriptions = prescriptionData,
        //        Diagnoses = diagnoseData
        //    };

        //    //add event log
        //    await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, response.Id, "PostAllMedicalRecords", MethodType.Update, nameof(MedicalRecordsDetailResponse), JsonConvert.SerializeObject(response));
        //    return response;
        //}
        public async Task<MedicalRecordsDetailResponse> PostAllMedicalRecords(MedicalRecordsDetailRequest request, string dbName)
        {
            var currentUserId = await _currentUser.UserId;
            var medicalRecords = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, request.MedicalRecordsId);
            var appointment = await _unitOfWork.AppointmentRepository.GetById(dbName, medicalRecords.AppointmentId);
            var staff = await _unitOfWork.ProfileRepository.GetById(dbName, medicalRecords.StaffId);

            bool isOpname = request.IsOpname;

            if (!isOpname)
            {
                appointment.StatusId = 4; // Update status to Pharmacy
                FormatUtil.SetDateBaseEntity(appointment, true);
                await _unitOfWork.AppointmentRepository.Update(dbName, appointment);

                var newAppointmentActivity = new AppointmentsActivity
                {
                    AppointmentId = appointment.Id,
                    CurrentDate = DateTime.Now,
                    CurrentStatusId = appointment.StatusId,
                    StaffId = medicalRecords.StaffId,
                    Note = string.Empty
                };
                FormatUtil.SetDateBaseEntity(newAppointmentActivity);
                await _unitOfWork.AppointmentRepository.AddActivity(newAppointmentActivity, dbName);

                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, appointment.Id, "PostAllMedicalRecords", MethodType.Update, nameof(Appointments), $"Update status to : {appointment.StatusId}");
            }

            var prescriptionData = new List<MedicalRecordsPrescriptions>();
            var diagnoseData = new List<MedicalRecordsDiagnoses>();

            medicalRecords.EndDate = DateTime.Now;

            if (request.Prescriptions.Any())
            {
                var currentTotal = medicalRecords.Total;

                if (isOpname)
                {
                    var currentPrescriptions = await _unitOfWork.MedicalRecordsPrescriptionsRepository.GetByMedicalRecordId(dbName, request.MedicalRecordsId);
                    var currentPresTotal = currentPrescriptions.Sum(x => x.Total);

                    currentTotal -= currentPresTotal;
                    await _unitOfWork.MedicalRecordsPrescriptionsRepository.RemoveRange(dbName, currentPrescriptions);
                }

                var totalPrescription = request.Prescriptions.Sum(x => x.Total);
                medicalRecords.Total = currentTotal + totalPrescription;

                foreach (var pItem in request.Prescriptions)
                {
                    FormatUtil.TrimObjectProperties(pItem);
                    var entity = Mapping.Mapper.Map<MedicalRecordsPrescriptions>(pItem);
                    FormatUtil.SetIsActive(entity, true);
                    FormatUtil.SetDateBaseEntity(entity);
                    entity.MedicalRecordsId = medicalRecords.Id;
                    var newId = await _unitOfWork.MedicalRecordsPrescriptionsRepository.Add(dbName, entity);
                    entity.Id = newId;
                    prescriptionData.Add(entity);
                }
            }

            FormatUtil.SetDateBaseEntity(medicalRecords);
            await _unitOfWork.MedicalRecordsRepository.Update(dbName, medicalRecords);

            if (request.Notes != null)
            {
                if (isOpname)
                {
                    var currentNotes = await _unitOfWork.MedicalRecordsNotesRepository.GetByMedicalRecordId(dbName, request.MedicalRecordsId);
                    await _unitOfWork.MedicalRecordsNotesRepository.RemoveRange(dbName, currentNotes);
                }

                FormatUtil.TrimObjectProperties(request.Notes);
                var notesEntity = Mapping.Mapper.Map<MedicalRecordsNotes>(request.Notes);
                FormatUtil.SetIsActive(notesEntity, true);
                FormatUtil.SetDateBaseEntity(notesEntity);
                notesEntity.MedicalRecordsId = medicalRecords.Id;
                notesEntity.StaffId = medicalRecords.StaffId;
                var newId = await _unitOfWork.MedicalRecordsNotesRepository.Add(dbName, notesEntity);
                notesEntity.Id = newId;
            }

            var notes = await _unitOfWork.MedicalRecordsNotesRepository.GetByMedicalRecordId(dbName, medicalRecords.Id);

            if (isOpname)
            {
                var currentDiagnoses = await _unitOfWork.MedicalRecordsDiagnosesRepository.GetByMedicalRecordId(dbName, request.MedicalRecordsId);
                await _unitOfWork.MedicalRecordsDiagnosesRepository.RemoveRange(dbName, currentDiagnoses);
            }

            foreach (var dItem in request.Diagnoses)
            {
                FormatUtil.TrimObjectProperties(dItem);
                var entity = Mapping.Mapper.Map<MedicalRecordsDiagnoses>(dItem);
                FormatUtil.SetIsActive(entity, true);
                FormatUtil.SetDateBaseEntity(entity);
                entity.MedicalRecordsId = medicalRecords.Id;
                var newId = await _unitOfWork.MedicalRecordsDiagnosesRepository.Add(dbName, entity);
                entity.Id = newId;
                diagnoseData.Add(entity);

                var existDiagnose = await _unitOfWork.DiagnoseRepository.GetByFilter(dbName, new NameBaseEntityFilter { Name = dItem.Diagnose });
                if (existDiagnose.TotalData < 1)
                {
                    var newDiagnose = new Diagnoses { Name = dItem.Diagnose };
                    _logger.LogInformation($"Try add new diagnose: {JsonConvert.SerializeObject(newDiagnose)}");
                    FormatUtil.SetIsActive(newDiagnose, true);
                    FormatUtil.SetDateBaseEntity(newDiagnose);
                    var newDiagnoseId = await _unitOfWork.DiagnoseRepository.Add(dbName, newDiagnose);
                    newDiagnose.Id = newDiagnoseId;
                }
            }

            var response = new MedicalRecordsDetailResponse
            {
                Id = medicalRecords.Id,
                Code = medicalRecords.Code,
                Staff = staff,
                Notes = notes,
                DiscountMethod = medicalRecords.DiscountMethod,
                DiscountValue = medicalRecords.DiscountValue,
                DiscountTotal = medicalRecords.DiscountTotal,
                TotalDiscounted = medicalRecords.TotalDiscounted,
                StartDate = medicalRecords.StartDate,
                EndDate = medicalRecords.EndDate.Value,
                TotalPrice = medicalRecords.Total,
                Prescriptions = prescriptionData,
                Diagnoses = diagnoseData
            };

            await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, response.Id, "PostAllMedicalRecords", MethodType.Update, nameof(MedicalRecordsDetailResponse), JsonConvert.SerializeObject(response));
            return response;
        }

        public async Task<MedicalRecordsNotesResponse> PostMedicalRecordsNotes(MedicalRecordsNotesRequest request, string email, string dbName)
        {
            try
            {
                var currentUserId = await _currentUser.UserId;
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<MedicalRecordsNotes>(request);
                FormatUtil.SetIsActive<MedicalRecordsNotes>(entity, true);
                var staff = await _unitOfWork.ProfileRepository.GetByEmail(dbName, email);
                entity.StaffId = staff.Id;

                var checkType = await _unitOfWork.MedicalRecordsNotesRepository.CheckRecordType(dbName, request.MedicalRecordsId, request.Type);
                var noteId = 0;
                if (checkType == null)
                {
                    FormatUtil.SetDateBaseEntity<MedicalRecordsNotes>(entity);
                    noteId = await _unitOfWork.MedicalRecordsNotesRepository.Add(dbName, entity);

                    //add event log
                    await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, noteId, "PostMedicalRecordsNotes", MethodType.Create, nameof(MedicalRecordsNotes));
                }
                else
                {
                    noteId = checkType.Id;
                    entity.Id = checkType.Id;
                    FormatUtil.SetDateBaseEntity<MedicalRecordsNotes>(entity, true);
                    await _unitOfWork.MedicalRecordsNotesRepository.Update(dbName, entity);

                    //add event log
                    await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, entity.Id, "PostMedicalRecordsNotes", MethodType.Update, nameof(MedicalRecordsNotes));
                }

                var response = new MedicalRecordsNotesResponse
                {
                    Id = noteId,
                    MedicalRecordsId = request.MedicalRecordsId,
                    StaffId = entity.StaffId,
                    Title = entity.Title,
                    Type = entity.Type,
                    Value = entity.Value
                };
                return response;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreateAnimalAsync";
                throw;
            }
        }

        public async Task<MedicalRecordsNotesResponse> PutMedicalRecordsNotes(int id, MedicalRecordsNotesRequest request, string email, string dbName)
        {
            try
            {
                var currentUserId = await _currentUser.UserId;
                var staff = await _unitOfWork.ProfileRepository.GetByEmail(dbName, email);

                var checkType = await _unitOfWork.MedicalRecordsNotesRepository.CheckRecordType(dbName, request.MedicalRecordsId, request.Type);
                var noteId = checkType.Id;


                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<MedicalRecordsNotes>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<MedicalRecordsNotes>(entity, true);

                MedicalRecordsNotes checkedEntity = await _unitOfWork.MedicalRecordsNotesRepository.GetById(dbName, id);
                if (checkedEntity.MedicalRecordsId != request.MedicalRecordsId) throw new Exception("Invalid medical records note");
                FormatUtil.ConvertUpdateObject<MedicalRecordsNotes, MedicalRecordsNotes>(entity, checkedEntity);
                FormatUtil.SetIsActive<MedicalRecordsNotes>(checkedEntity, true);
                await _unitOfWork.MedicalRecordsNotesRepository.Update(dbName, checkedEntity);

                //add event log
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, checkedEntity.Id, "PutMedicalRecordsNotes", MethodType.Update, nameof(MedicalRecordsNotes));

                var response = new MedicalRecordsNotesResponse
                {
                    Id = noteId,
                    MedicalRecordsId = request.MedicalRecordsId,
                    StaffId = staff.Id,
                    Title = request.Title,
                    Type = request.Type,
                    Value = request.Value
                };
                return response;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.PutMedicalRecordsNotes";
                throw;
            }
        }
        public async Task<IEnumerable<MedicalRecordsNotes>> GetMedicalRecordsNotes(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _unitOfWork.MedicalRecordsNotesRepository.GetByMedicalRecordId(dbName, id);
                if (entity == null) throw new Exception("Entity not found");
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"MedicalRecordService.GetMedicalRecordsNotes";
                throw;
            }
        }

        public async Task DeleteMedicalRecordsNotes(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _unitOfWork.MedicalRecordsNotesRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _unitOfWork.MedicalRecordsNotesRepository.Remove(dbName, id);

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "DeleteMedicalRecordsNotes", MethodType.Delete, nameof(MedicalRecordsNotes));
            }
            catch (Exception ex)
            {
                ex.Source = $"MedicalRecordService.DeleteMedicalRecordsNotes";
                throw;
            }
        }

        public async Task<MedicalDocsRequirementResponse> GetMedicalRecordRequirement(int medicalRecordId, string dbName)
        {
            try
            {
                var responseClinic = await _unitOfWork.ClinicsRepository.GetAll(dbName);
                var responseMedicalRecords = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, medicalRecordId);
                var responseVet = await _unitOfWork.ProfileRepository.GetById(dbName, responseMedicalRecords.StaffId);
                var responseAppointment = await _unitOfWork.AppointmentRepository.GetById(dbName, responseMedicalRecords.AppointmentId);
                var responsePatient = await _unitOfWork.PatientsRepository.GetById(dbName, responseAppointment.PatientsId);
                var responsePatientStatistic = await _unitOfWork.PatientsStatisticRepository.ReadPatientsStatisticAsync(responseAppointment.PatientsId, dbName);
                var patientStatistic = await GetPatientStatisticLatest(responsePatientStatistic, dbName);
                var responseOwner = await _unitOfWork.OwnersRepository.GetById(dbName, responseAppointment.OwnersId);
                var responseDiagnose = await _unitOfWork.MedicalRecordsDiagnosesRepository.GetByMedicalRecordId(dbName, responseMedicalRecords.Id);
                var response = new MedicalDocsRequirementResponse
                {
                    ClinicData = responseClinic.First(),
                    MedicalData = responseMedicalRecords,
                    OwnerData = responseOwner,
                    PatientData = responsePatient,
                    PatientLatestStatistic = patientStatistic,
                    MedicalDiagnoses = responseDiagnose,
                    VetName = responseVet.Name
                };
                return response;
            }
            catch (Exception ex)
            {
                ex.Source = $"MedicalRecordService.GetMedicalRecordRequirement";
                throw;
            }
        }

        private async Task<IEnumerable<PatientsStatisticResponse>> GetPatientStatisticLatest(IEnumerable<PatientsStatisticDto> dtoData, string dbName)
        {
            var result = new List<PatientsStatisticResponse>();
            foreach (var item in dtoData)
            {
                string change;
                string beforeValueText = item.Before != null ? $"{item.Before} {item.Unit}" : "null";
                if (item.Type == "Blood Pressure")
                {
                    change = "";
                }
                else if (item.Before == null)
                {
                    change = null;
                }
                else
                {
                    double latestValue = double.Parse(item.Latest);
                    double beforeValue = double.Parse(item.Before);
                    double changeValue = latestValue - beforeValue;
                    double changePercentage = (latestValue / beforeValue) * 100;
                    change = $"{changeValue} ({changePercentage.ToString("0.00")}%)";
                }
                var checkedAt = FormatUtil.GetTimeAgo(item.CreatedAt);
                var staff = await _unitOfWork.ProfileRepository.GetById(dbName, item.StaffId);
                var newResponse = new PatientsStatisticResponse()
                {
                    Latest = $"{item.Latest} {item.Unit}",
                    Before = beforeValueText,
                    Change = change,
                    PatientId = item.PatientId,
                    Type = item.Type,
                    CheckedAt = checkedAt,
                    CheckedBy = staff.Name
                };
                result.Add(newResponse);
            }
            return result;
        }
        public async Task<OrdersPaymentResponse> AddOrdersPaymentAsync(OrdersPaymentRequest request, string dbName)
        {
            try
            {
                var currentUserId = await _currentUser.UserId;
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<OrdersPayment>(request);
                FormatUtil.SetIsActive<OrdersPayment>(entity, true);
                FormatUtil.SetDateBaseEntity<OrdersPayment>(entity);
                entity.Type = "MedicalRecord";
                var lastPayments = await _unitOfWork.OrdersPaymentRepository.GetPaidByOrderId(dbName, request.OrderId, entity.Type);
                var getTotalLastPayment = lastPayments.Sum(x => x.Total);

                var medicalRecord = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, request.OrderId);
                if (medicalRecord == null) throw new Exception("MedicalRecord not found");

                var totalPrice = medicalRecord.Total;
                if (request.DiscountTotal.HasValue)
                {
                    medicalRecord.DiscountMethod = request.DiscountMethod;
                    medicalRecord.DiscountValue = request.DiscountValue;
                    medicalRecord.DiscountTotal = request.DiscountTotal;
                    medicalRecord.TotalDiscounted = medicalRecord.Total - request.DiscountTotal;

                    totalPrice = medicalRecord.Total - request.DiscountTotal.Value;

                    FormatUtil.SetDateBaseEntity<MedicalRecords>(medicalRecord, true);
                    await _unitOfWork.MedicalRecordsRepository.Update(dbName, medicalRecord);
                }

                //update status
                var paymentStatus = "Paid";
                entity.Status = paymentStatus;
                var newId = await _unitOfWork.OrdersPaymentRepository.Add(dbName, entity);
                entity.Id = newId;

                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "AddOrdersPaymentAsync", MethodType.Create, nameof(OrdersPayment));

                var totalPayments = getTotalLastPayment + request.Total;
                if ((totalPayments) >= totalPrice)
                {
                    medicalRecord.PaymentStatus = paymentStatus;
                    FormatUtil.SetDateBaseEntity<MedicalRecords>(medicalRecord, true);
                    await _unitOfWork.MedicalRecordsRepository.Update(dbName, medicalRecord);

                    var prescriptions = await _unitOfWork.MedicalRecordsPrescriptionsRepository.GetByMedicalRecordId(dbName, medicalRecord.Id);

                    foreach (var item in prescriptions)
                    {
                        if (item.Type == "Product")
                        {
                            var productStock = await _unitOfWork.ProductStockRepository.WhereFirstQuery(dbName, $"ProductId = {item.ProductId}");
                            var tuple = StockUtil.CalculateProductStockMinVolume(productStock, item.PrescriptionAmount);
                            tuple.Item2.Type = "MedicalRecord";
                            tuple.Item2.ProfileId = medicalRecord.StaffId;

                            FormatUtil.SetIsActive<ProductStockHistorical>(tuple.Item2, true);
                            FormatUtil.SetDateBaseEntity<ProductStockHistorical>(tuple.Item2);

                            await _unitOfWork.ProductStockRepository.Update(dbName, tuple.Item1);
                            await _unitOfWork.ProductStockHistoricalRepository.Add(dbName, tuple.Item2);
                        }
                    }
                }
                else
                {
                    paymentStatus = "Paid Less";
                    medicalRecord.PaymentStatus = paymentStatus;
                    FormatUtil.SetDateBaseEntity<MedicalRecords>(medicalRecord, true);
                    await _unitOfWork.MedicalRecordsRepository.Update(dbName, medicalRecord);
                }

                //add event log
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, request.OrderId, "AddOrdersPaymentAsync", MethodType.Create, nameof(OrdersPaymentRequest), JsonConvert.SerializeObject(request));
                
                var result = new OrdersPaymentResponse
                {
                    Date = request.Date,
                    OrderId = request.OrderId,
                    PaymentMethodId = request.PaymentMethodId,
                    Total = request.Total,
                    LessTotal = totalPrice - totalPayments,
                    Status = paymentStatus,
                    Type = entity.Type
                };
                return result;
            }
            catch (Exception ex)
            {
                ex.Source = $"MedicalRecordService.AddOrdersPaymentAsync";
                throw;
            }
        }

        public async Task<IEnumerable<OrdersPayment>> GetOrdersPaymentAsync(int medicalRecordId, string dbName)
        {
            try
            {
                var type = "MedicalRecord";
                var lastPayments = await _unitOfWork.OrdersPaymentRepository.GetPaidByOrderId(dbName, medicalRecordId, type);
                var getTotalLastPayment = lastPayments.Sum(x => x.Total);

                return lastPayments;
            }
            catch (Exception ex)
            {
                ex.Source = $"OrderService.GetOrdersPaymentAsync";
                throw;
            }
        }

        public async Task<IEnumerable<PatientDiagnosesResponse>> GetPatientDiagnose(int patientId, string dbName)
        {
            var result = new List<PatientDiagnosesResponse>();
            var mapData = new List<string>();
            var data = await _unitOfWork.AppointmentRepository.GetBookingHistoryPatient(dbName, patientId);
            if (data.Count() > 0)
            {
                foreach (var item in data.Select(x => x.MedicalRecordsId))
                {
                    var diagnoses = await _unitOfWork.MedicalRecordsDiagnosesRepository.GetByMedicalRecordId(dbName, item);
                    mapData.AddRange(diagnoses.Select(x => x.Diagnose));
                }

                if (mapData.Count() > 0)
                {
                    result = mapData.GroupBy(x => x).Select(g => new PatientDiagnosesResponse
                    {
                        Diagnose = g.Key,
                        Count = g.Count(),
                        Percentage = (g.Count() / (double)mapData.Count()) * 100
                    }).ToList();
                }
            }
            return result;
        }

        public async Task<OpnamePatients> PostCloseOpname(int medId, string dbName)
        {
            var currentUserId = await _currentUser.UserId;
            var getMedical = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, medId);
            if (getMedical == null) throw new Exception("Medical record not found");
            var getPatientOpname = await _unitOfWork.OpnamePatientsRepository.GetByMedId(dbName, medId); 
            var dataOpnamePatients = getPatientOpname.Data.FirstOrDefault();
            if (dataOpnamePatients == null) throw new Exception("Opname not found");
            if (dataOpnamePatients.Status == "Done") throw new Exception("Opname invalid");
            var getAppointment = await _unitOfWork.AppointmentRepository.GetById(dbName, getMedical.AppointmentId);
            if (getAppointment == null) throw new Exception("Appointment not found");
            if (getAppointment.StatusId != 3) throw new Exception("Appointment invalid");

            var getOpname = await _unitOfWork.OpnamesRepository.GetById(dbName, dataOpnamePatients.OpnameId);
            var duration = FormatUtil.CalculateDaysBetween(dataOpnamePatients.StartTime, DateTime.Now);

            //update status patient opname to done
            dataOpnamePatients.Status = "Done";
            dataOpnamePatients.EndTime = DateTime.Now;
            dataOpnamePatients.TotalPrice = dataOpnamePatients.Price * duration;
            FormatUtil.SetDateBaseEntity(dataOpnamePatients, true);
            await _unitOfWork.OpnamePatientsRepository.Update(dbName, dataOpnamePatients);

            //add opname as prescription
            var newPrescription = new MedicalRecordsPrescriptions()
            {
                MedicalRecordsId = getMedical.Id,
                PrescriptionAmount = duration, //duration
                PrescriptionFrequency = "",
                ProductId = 0,
                Type = "Service",
                ProductName = getOpname.Name,
                Price = dataOpnamePatients.Price,
                Total = dataOpnamePatients.TotalPrice,
                Quantity = duration //duration
            };

            FormatUtil.TrimObjectProperties(newPrescription);
            var entity = Mapping.Mapper.Map<MedicalRecordsPrescriptions>(newPrescription);
            FormatUtil.SetIsActive<MedicalRecordsPrescriptions>(entity, true);
            FormatUtil.SetDateBaseEntity<MedicalRecordsPrescriptions>(entity);
            var newId = await _unitOfWork.MedicalRecordsPrescriptionsRepository.Add(dbName, entity);
            entity.Id = newId;

            //update medicalRecords total
            var totalNow = getMedical.Total + newPrescription.Total;
            getMedical.Total = totalNow;
            FormatUtil.SetDateBaseEntity(getMedical, true);
            await _unitOfWork.MedicalRecordsRepository.Update(dbName, getMedical);

            //update appointment status to pharmacy (4)
            getAppointment.StatusId = 4; // Update status to Pharmacy
            FormatUtil.SetDateBaseEntity(getAppointment, true);
            await _unitOfWork.AppointmentRepository.Update(dbName, getAppointment);

            var newAppointmentActivity = new AppointmentsActivity
            {
                AppointmentId = getAppointment.Id,
                CurrentDate = DateTime.Now,
                CurrentStatusId = getAppointment.StatusId,
                StaffId = getMedical.StaffId,
                Note = string.Empty
            };
            FormatUtil.SetDateBaseEntity(newAppointmentActivity);
            await _unitOfWork.AppointmentRepository.AddActivity(newAppointmentActivity, dbName);

            await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, dataOpnamePatients.Id, "PostCloseOpname", MethodType.Update, nameof(OpnamePatients), $"Update status to : Done");
            return dataOpnamePatients;
        }
    }
}
