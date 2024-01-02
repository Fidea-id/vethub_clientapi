using Application.Services.Contracts;
using Application.Utils;
using AutoMapper;
using Domain.Entities.DTOs;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Models.Masters;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Services.Implementations
{
    public class MedicalRecordService : GenericService<MedicalRecords, MedicalRecordsRequest, MedicalRecordsResponse, MedicalRecordsFilter>, IMedicalRecordService
    {
        private readonly ILogger<MedicalRecordService> _logger;
        public MedicalRecordService(IUnitOfWork unitOfWork, IGenericRepository<MedicalRecords, MedicalRecordsFilter> repository,
            ILoggerFactory loggerFactory)
        : base(unitOfWork, repository)
        {
            _logger = loggerFactory.CreateLogger<MedicalRecordService>();
        }

        public async Task<MedicalRecordsDetailResponse> GetDetailMedicalRecords(int id, string dbName)
        {
            var medicalRecords = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, id);
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
            string statusPayment = "Paid";
            if(lastPayments.Count() < 1)
            {
                statusPayment = "Unpaid";
            }
            else if(totalLastPayment < medicalRecords.Total)
            {
                statusPayment = "Paid Less";
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
                EndDate = medicalRecords.EndDate.Value,
                TotalPrice = medicalRecords.Total,
                TotalPaid = totalLastPayment,
                Prescriptions = presciptions,
                Diagnoses = diagnoses,
                StatusPayment = statusPayment,
                Notes = notes
            };
            return response;
        }
        public async Task<DataResultDTO<BookingHistoryResponse>> GetBookingHistoryByOwner(int ownerId, string dbName)
        {
            var data = await _unitOfWork.AppointmentRepository.GetBookingHistoryOwner(dbName, ownerId);
            if(data.Count() > 0)
            {
                foreach(var item in data)
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

        public async Task<MedicalRecordsDetailResponse> PostAllMedicalRecords(MedicalRecordsDetailRequest request, string dbName)
        {
            var medicalRecords = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, request.MedicalRecordsId);
            var appointment = await _unitOfWork.AppointmentRepository.GetById(dbName, medicalRecords.AppointmentId);
            var staff = await _unitOfWork.ProfileRepository.GetById(dbName, medicalRecords.StaffId);
            //update appointment status to pharmacy
            var currentStatus = 4;
            appointment.StatusId = currentStatus;
            FormatUtil.SetDateBaseEntity<MedicalRecords>(medicalRecords, true);
            await _unitOfWork.AppointmentRepository.Update(dbName, appointment);

            // add appointment activity
            var newAppointment = new AppointmentsActivity()
            {
                AppointmentId = appointment.Id,
                CurrentDate = DateTime.Now,
                CurrentStatusId = currentStatus,
                StaffId = medicalRecords.StaffId,
                Note = ""
            };
            FormatUtil.SetDateBaseEntity<AppointmentsActivity>(newAppointment);
            await _unitOfWork.AppointmentRepository.AddActivity(newAppointment, dbName);

            // update data medical records
            medicalRecords.EndDate = DateTime.Now;
            var currentTotal = medicalRecords.Total;
            var totalPrescription = request.Prescriptions.Sum(x => x.Total);
            medicalRecords.Total = currentTotal + totalPrescription;
            FormatUtil.SetDateBaseEntity<MedicalRecords>(medicalRecords);
            await _unitOfWork.MedicalRecordsRepository.Update(dbName, medicalRecords);

            var prescriptionData = new List<MedicalRecordsPrescriptions>();
            var diagnoseData = new List<MedicalRecordsDiagnoses>();

            // add data notes
            if(request.Notes != null)
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request.Notes);
                var entity = Mapping.Mapper.Map<MedicalRecordsNotes>(request.Notes);
                FormatUtil.SetIsActive<MedicalRecordsNotes>(entity, true);
                FormatUtil.SetDateBaseEntity<MedicalRecordsNotes>(entity);
                entity.MedicalRecordsId = medicalRecords.Id;
                entity.StaffId = medicalRecords.StaffId;
                var newId = await _unitOfWork.MedicalRecordsNotesRepository.Add(dbName, entity);
                entity.Id = newId;
            }

            var notes = await _unitOfWork.MedicalRecordsNotesRepository.GetByMedicalRecordId(dbName, medicalRecords.Id);

            // add data prescription
            foreach (var pItem in request.Prescriptions)
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
            }

            // add data diagnoses
            foreach (var dItem in request.Diagnoses)
            {
                //trim all string
                FormatUtil.TrimObjectProperties(dItem);
                var entity = Mapping.Mapper.Map<MedicalRecordsDiagnoses>(dItem);
                FormatUtil.SetIsActive<MedicalRecordsDiagnoses>(entity, true);
                FormatUtil.SetDateBaseEntity<MedicalRecordsDiagnoses>(entity);
                entity.MedicalRecordsId = medicalRecords.Id;
                var newId = await _unitOfWork.MedicalRecordsDiagnosesRepository.Add(dbName, entity);
                entity.Id = newId;
                diagnoseData.Add(entity);

                //check new diagnose
                var existDiagnose = await _unitOfWork.DiagnoseRepository.GetByFilter(dbName, new NameBaseEntityFilter() { Name = dItem.Diagnose });
                if (existDiagnose.TotalData < 1)
                {
                    var newDiagnose = new Diagnoses() 
                    { 
                        Name = dItem.Diagnose,
                    };
                    _logger.LogInformation("Try add new diagnose : " + JsonConvert.SerializeObject(newDiagnose));
                    FormatUtil.SetIsActive<Diagnoses>(newDiagnose, true);
                    FormatUtil.SetDateBaseEntity<Diagnoses>(newDiagnose);

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
                StartDate = medicalRecords.StartDate,
                EndDate = medicalRecords.EndDate.Value,
                TotalPrice = medicalRecords.Total,
                Prescriptions = prescriptionData,
                Diagnoses = diagnoseData
            };
            return response;
        }

        public async Task<MedicalRecordsNotesResponse> PostMedicalRecordsNotes(MedicalRecordsNotesRequest request, string email, string dbName)
        {
            try
            {
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
                }
                else
                {
                    noteId = checkType.Id;
                    entity.Id = checkType.Id;
                    FormatUtil.SetDateBaseEntity<MedicalRecordsNotes>(entity, true);
                    await _unitOfWork.MedicalRecordsNotesRepository.Update(dbName, entity);
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

                var paymentStatus = "Paid";
                entity.Status = paymentStatus;
                var newId = await _unitOfWork.OrdersPaymentRepository.Add(dbName, entity);
                entity.Id = newId;
                //update status
                var totalPayments = getTotalLastPayment + request.Total;
                if ((totalPayments) >= medicalRecord.Total)
                {
                    medicalRecord.PaymentStatus = paymentStatus;
                    FormatUtil.SetDateBaseEntity<MedicalRecords>(medicalRecord, true);
                    await _unitOfWork.MedicalRecordsRepository.Update(dbName, medicalRecord);

                    var prescriptions = await _unitOfWork.MedicalRecordsPrescriptionsRepository.GetByMedicalRecordId(dbName, medicalRecord.Id);

                    foreach (var item in prescriptions)
                    {
                        if(item.Type == "Product")
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

                var result = new OrdersPaymentResponse
                {
                    Date = request.Date,
                    OrderId = request.OrderId,
                    PaymentMethodId = request.PaymentMethodId,
                    Total = request.Total,
                    LessTotal = medicalRecord.Total - totalPayments,
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

                if(mapData.Count() > 0)
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
    }
}
