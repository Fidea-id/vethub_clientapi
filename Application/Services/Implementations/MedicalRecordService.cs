using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application.Services.Implementations
{
    public class MedicalRecordService : GenericService<MedicalRecords, MedicalRecordsRequest, MedicalRecordsResponse, MedicalRecordsFilter>, IMedicalRecordService
    {
        public MedicalRecordService(IUnitOfWork unitOfWork, IGenericRepository<MedicalRecords, MedicalRecordsFilter> repository)
        : base(unitOfWork, repository)
        { }

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
                Prescriptions = presciptions,
                Diagnoses = diagnoses,
                Notes = notes
            };
            return response;
        }

        public async Task<MedicalRecordsDetailResponse> PostAllMedicalRecords(MedicalRecordsDetailRequest request, string dbName)
        {
            var medicalRecords = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, request.MedicalRecordsId);

            //TODO:update appointment status to pharmacy
            //TODO:add appointment activity


            // update data medical records
            medicalRecords.EndDate = DateTime.Now;
            var currentTotal = medicalRecords.Total;
            var totalPrescription = request.Prescriptions.Sum(x => x.Total);
            var totalDiagnose = request.Diagnoses.Sum(x => x.TotalPrice);
            medicalRecords.Total = currentTotal + totalDiagnose + totalPrescription;
            FormatUtil.SetDateBaseEntity<MedicalRecords>(medicalRecords);
            await _unitOfWork.MedicalRecordsRepository.Update(dbName, medicalRecords);

            var prescriptionData = new List<MedicalRecordsPrescriptions>();
            var diagnoseData = new List<MedicalRecordsDiagnoses>();

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
            }
            var response = new MedicalRecordsDetailResponse
            {
                Id = medicalRecords.Id,
                Code = medicalRecords.Code,
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
                var noteId = checkType.Id;
                if (checkType == null)
                {
                    FormatUtil.SetDateBaseEntity<MedicalRecordsNotes>(entity);
                    noteId = await _unitOfWork.MedicalRecordsNotesRepository.Add(dbName, entity);
                }
                else
                {
                    entity.Id = noteId;
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
                var response = new MedicalDocsRequirementResponse
                {
                    ClinicData = responseClinic.First(),
                    MedicalData = responseMedicalRecords,
                    OwnerData = responseOwner,
                    PatientData = responsePatient,
                    PatientLatestStatistic = patientStatistic,
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
    }
}
