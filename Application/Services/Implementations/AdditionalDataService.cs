using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.DTOs;
using Domain.Entities.Filters;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Domain.Entities.Filters.Clients;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services.Implementations
{
    public class AdditionalDataService : IAdditionalDataService
    {
        private readonly IUnitOfWork _uow;

        public AdditionalDataService(IUnitOfWork unitOfWork)
        {
            _uow = unitOfWork;
        }

        #region Dashboard
        public async Task<DashboardResponse> ReadDashboardAsync(string dbName, string date)
        {
            var statusPaid = "Paid";
            var clients = await _uow.OwnersRepository.CountToCard(dbName, date, null);
            var pets = await _uow.PatientsRepository.CountToCard(dbName, date, null);
            var appointment = await _uow.AppointmentRepository.CountToCard(dbName, date, null);

            var dateFilter = $"CreatedAt >= '2023-01-01'";
            if (!string.IsNullOrEmpty(date))
            {
                dateFilter = $"CreatedAt >= '{date}'";
            }

            var revenueAppointmentAll = await _uow.MedicalRecordsRepository.SumWithQuery(dbName, "Total", $"PaymentStatus = '{statusPaid}'");
            var revenueAppointmentMonth = await _uow.MedicalRecordsRepository.SumWithQuery(dbName, "Total", $"{dateFilter} AND PaymentStatus = '{statusPaid}'");
            var revenueOrderAll = await _uow.OrdersRepository.SumWithQuery(dbName, "TotalPrice", $"Status = '{statusPaid}'");
            var revenueOrderMonth = await _uow.OrdersRepository.SumWithQuery(dbName, "TotalPrice", $"{dateFilter} AND Status = '{statusPaid}'");

            var visitYearly = await _uow.MedicalRecordsRepository.GetVisitYearly(dbName);

            var visitYearlyConverted = visitYearly.Select(item => new MonthlyDataChart()
            {
                Month = DateTime.ParseExact(item.Month, "MM", null).ToString("MMM"),
                Year = item.Year,
                Total = item.Total
            });

            var revenueAll = revenueOrderAll + revenueAppointmentAll;
            var revenueMonth = revenueOrderMonth + revenueAppointmentMonth;

            var result = new DashboardResponse();
            clients.Percentage = FormatUtil.CountPercentageMonth(clients.Total, clients.TotalAll);
            result.Clients = clients;

            pets.Percentage = FormatUtil.CountPercentageMonth(pets.Total, pets.TotalAll);
            result.Pets = pets;

            appointment.Percentage = FormatUtil.CountPercentageMonth(appointment.Total, appointment.TotalAll);
            result.Appointments = appointment;

            result.Revenues = new CardDashboard()
            {
                Total = revenueMonth,
                TotalAll = revenueAll,
                Percentage = FormatUtil.CountPercentageMonth(revenueMonth, revenueAll)
            };

            //appointment activity & order activity
            var listActivities = new List<ActivityDashboard>();
            var appointmentDetail = await _uow.AppointmentRepository.GetAllDetailList(dbName, null);
            var latestAppointment = appointmentDetail.Data.OrderByDescending(x => x.Date).Take(10);
            foreach(var item in latestAppointment)
            {
                var newActivity = new ActivityDashboard()
                {
                    Time = item.Date,
                    Type = "Appointment",
                    Name = $"{item.PatientsName}({item.OwnersName})",
                    Detail = $"{item.ServiceName} with {item.StaffName}"
                };
                listActivities.Add(newActivity);
            }
            result.Activities = listActivities;

            //chart overview
            result.ChartClients = new List<ChartDataSeries>();
            var visitChart = new ChartDataSeries()
            {
                Data = visitYearlyConverted,
                Name = "Visits"
            };
            result.ChartClients.Add(visitChart);

            return result;
        }
        #endregion

        #region Clinics
        public async Task<Clinics> CreateClinicsAsync(ClinicsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Clinics>(request);
                FormatUtil.SetIsActive<Clinics>(entity, true);
                FormatUtil.SetDateBaseEntity<Clinics>(entity);

                var newId = await _uow.ClinicsRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreateClinicsAsync";
                throw;
            }
        }
        public async Task<Clinics> ReadClinicsAsync(string dbName)
        {
            try
            {
                var data = await _uow.ClinicsRepository.GetAll(dbName);
                return data.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadByIdClinicsAsync";
                throw;
            }
        }
        public async Task<Clinics> UpdateClinicsAsync(int id, ClinicsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Clinics>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<Clinics>(entity, true);

                Clinics checkedEntity = await _uow.ClinicsRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<Clinics, Clinics>(entity, checkedEntity);
                FormatUtil.SetIsActive<Clinics>(checkedEntity, true);
                await _uow.ClinicsRepository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdateClinicsAsync";
                throw;
            }
        }
        #endregion

        #region Animals
        public async Task<Animals> CreateAnimalAsync(AnimalsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Animals>(request);
                FormatUtil.SetIsActive<Animals>(entity, true);
                FormatUtil.SetDateBaseEntity<Animals>(entity);

                //check name first
                var checkedEntity = await _uow.AnimalRepository.GetByName(dbName, request.Name);
                if (checkedEntity != null) throw new Exception($"Already have {request.Name} as species");

                var newId = await _uow.AnimalRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreateAnimalAsync";
                throw;
            }
        }
        public async Task<DataResultDTO<Animals>> ReadAnimalAllAsync(NameBaseEntityFilter filter, string dbName)
        {
            try
            {
                var data = await _uow.AnimalRepository.GetByFilter(dbName, filter);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadAllAnimalAsync";
                throw;
            }
        }
        public async Task<Animals> ReadAnimalByIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _uow.AnimalRepository.GetById(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadByIdAnimalAsync";
                throw;
            }
        }
        public async Task<Animals> UpdateAnimalAsync(int id, AnimalsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Animals>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<Animals>(entity, true);

                Animals checkedEntity = await _uow.AnimalRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<Animals, Animals>(entity, checkedEntity);
                FormatUtil.SetIsActive<Animals>(checkedEntity, true);
                await _uow.AnimalRepository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdateAnimalAsync";
                throw;
            }
        }
        public async Task DeleteAnimalAsync(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _uow.AnimalRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _uow.AnimalRepository.Remove(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.DeleteAnimalAsync";
                throw;
            }
        }
        #endregion

        #region Breed
        public async Task<Breeds> CreateBreedAsync(BreedsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Breeds>(request);
                FormatUtil.SetIsActive<Breeds>(entity, true);
                FormatUtil.SetDateBaseEntity<Breeds>(entity);

                //check name first
                var checkedEntity = await _uow.BreedRepository.GetByName(dbName, request.AnimalsId, request.Name);
                if (checkedEntity != null) throw new Exception($"Already have {request.Name} as breeds at selected species");

                var newId = await _uow.BreedRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreateBreedAsync";
                throw;
            }
        }
        public async Task<DataResultDTO<BreedAnimalResponse>> ReadBreedAllAsync(NameBaseEntityFilter filter, string dbName)
        {
            try
            {
                var data = await _uow.BreedRepository.GetBreedAnimalList(filter, dbName);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadAllAnimalAsync";
                throw;
            }
        }

        public async Task<IEnumerable<BreedAnimalResponse>> ReadBreedByIdAnimalAsync(int idAnimal, string dbName)
        {
            try
            {
                var data = await _uow.BreedRepository.GetBreedAnimalListByAnimal(idAnimal, dbName);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadByIdAnimalAsync";
                throw;
            }
        }

        public async Task<BreedAnimalResponse> ReadBreedByIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _uow.BreedRepository.GetBreedAnimal(id, dbName);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadByIdAnimalAsync";
                throw;
            }
        }
        public async Task<Breeds> UpdateBreedAsync(int id, BreedsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Breeds>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<Breeds>(entity, true);

                Breeds checkedEntity = await _uow.BreedRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<Breeds, Breeds>(entity, checkedEntity);
                FormatUtil.SetIsActive<Breeds>(checkedEntity, true);
                await _uow.BreedRepository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdateBreedAsync";
                throw;
            }
        }
        public async Task DeleteBreedAsync(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _uow.BreedRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _uow.BreedRepository.Remove(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.DeleteBreedAsync";
                throw;
            }
        }
        #endregion

        #region Diagnoses
        public async Task<Diagnoses> CreateDiagnoseAsync(DiagnosesRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Diagnoses>(request);
                FormatUtil.SetIsActive<Diagnoses>(entity, true);
                FormatUtil.SetDateBaseEntity<Diagnoses>(entity);

                var newId = await _uow.DiagnoseRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreateDiagnoseAsync";
                throw;
            }
        }
        public async Task<DataResultDTO<Diagnoses>> ReadDiagnoseAllAsync(NameBaseEntityFilter filter, string dbName)
        {
            try
            {
                var data = await _uow.DiagnoseRepository.GetByFilter(dbName, filter);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadAllDiagnoseAsync";
                throw;
            }
        }
        public async Task<Diagnoses> ReadDiagnoseByIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _uow.DiagnoseRepository.GetById(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadByIdDiagnoseAsync";
                throw;
            }
        }
        public async Task<Diagnoses> UpdateDiagnoseAsync(int id, DiagnosesRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Diagnoses>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<Diagnoses>(entity, true);

                Diagnoses checkedEntity = await _uow.DiagnoseRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<Diagnoses, Diagnoses>(entity, checkedEntity);
                FormatUtil.SetIsActive<Diagnoses>(checkedEntity, true);
                await _uow.DiagnoseRepository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdateDiagnoseAsync";
                throw;
            }
        }
        public async Task DeleteDiagnoseAsync(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _uow.DiagnoseRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _uow.DiagnoseRepository.Remove(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.DeleteDiagnoseAsync";
                throw;
            }
        }
        #endregion

        #region PaymentMethod
        public async Task<PaymentMethod> CreatePaymentMethodAsync(PaymentMethodRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<PaymentMethod>(request);
                FormatUtil.SetIsActive<PaymentMethod>(entity, true);
                FormatUtil.SetDateBaseEntity<PaymentMethod>(entity);

                var newId = await _uow.PaymentMethodRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreatePaymentMethodAsync";
                throw;
            }
        }
        public async Task<DataResultDTO<PaymentMethod>> ReadPaymentMethodAllAsync(NameBaseEntityFilter filter, string dbName)
        {
            try
            {
                var data = await _uow.PaymentMethodRepository.GetByFilter(dbName, filter);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadAllPaymentMethodAsync";
                throw;
            }
        }
        public async Task<PaymentMethod> ReadPaymentMethodByIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _uow.PaymentMethodRepository.GetById(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadByIdPaymentMethodAsync";
                throw;
            }
        }
        public async Task<PaymentMethod> UpdatePaymentMethodAsync(int id, PaymentMethodRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<PaymentMethod>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<PaymentMethod>(entity, true);

                PaymentMethod checkedEntity = await _uow.PaymentMethodRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<PaymentMethod, PaymentMethod>(entity, checkedEntity);
                FormatUtil.SetIsActive<PaymentMethod>(checkedEntity, true);
                await _uow.PaymentMethodRepository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdatePaymentMethodAsync";
                throw;
            }
        }
        public async Task DeletePaymentMethodAsync(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _uow.PaymentMethodRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _uow.PaymentMethodRepository.Remove(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.DeletePaymentMethodAsync";
                throw;
            }
        }
        #endregion

        #region PrescriptionFrequents
        public async Task<PrescriptionFrequents> CreatePrescriptionFrequentsAsync(PrescriptionFrequentsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<PrescriptionFrequents>(request);
                FormatUtil.SetIsActive<PrescriptionFrequents>(entity, true);
                FormatUtil.SetDateBaseEntity<PrescriptionFrequents>(entity);

                var newId = await _uow.PrescriptionFrequentsRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreatePrescriptionFrequentsAsync";
                throw;
            }
        }
        public async Task<DataResultDTO<PrescriptionFrequents>> ReadPrescriptionFrequentsAllAsync(PrescriptionFrequentsFilter filter, string dbName)
        {
            try
            {
                var data = await _uow.PrescriptionFrequentsRepository.GetByFilter(dbName, filter);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadAllPrescriptionFrequentsAsync";
                throw;
            }
        }
        public async Task<PrescriptionFrequents> UpdatePrescriptionFrequentsAsync(int id, PrescriptionFrequentsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<PrescriptionFrequents>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<PrescriptionFrequents>(entity, true);

                PrescriptionFrequents checkedEntity = await _uow.PrescriptionFrequentsRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<PrescriptionFrequents, PrescriptionFrequents>(entity, checkedEntity);
                FormatUtil.SetIsActive<PrescriptionFrequents>(checkedEntity, true);
                await _uow.PrescriptionFrequentsRepository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdatePrescriptionFrequentsAsync";
                throw;
            }
        }
        public async Task DeletePrescriptionFrequentsAsync(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _uow.PrescriptionFrequentsRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _uow.PrescriptionFrequentsRepository.Remove(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.DeletePrescriptionFrequentsAsync";
                throw;
            }
        }
        #endregion
    }
}
