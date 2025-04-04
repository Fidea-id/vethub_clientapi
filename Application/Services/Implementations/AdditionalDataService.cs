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
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Eventing.Reader;
using Domain.Entities;
using Newtonsoft.Json;
using Infrastructure.Utils;
using DevExpress.XtraReports.Native;
using Domain.Entities.Models.Masters;
using System.Reflection;
using System;

namespace Application.Services.Implementations
{
    public class AdditionalDataService : IAdditionalDataService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AdditionalDataService> _logger;
        private readonly ICurrentUserService _currentUser;

        public AdditionalDataService(ICurrentUserService currentUser, IUnitOfWork unitOfWork, ILoggerFactory loggerFactory)
        {
            _currentUser = currentUser;
            _uow = unitOfWork;
            _logger = loggerFactory.CreateLogger<AdditionalDataService>();
        }

        #region Dashboard
        public async Task<DashboardResponse> ReadDashboardAsync(string dbName, string startDate, string endDate)
        {
            var statusPaid = "Paid";
            var defaultFilter = " CreatedAt >= '2023-01-01'";
            var dateFilter = defaultFilter;
            var dateFilterMR = " mr.StartDate >= '2023-01-01'";

            // Handle single or range date filtering
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                dateFilter = $" CreatedAt BETWEEN '{startDate}' AND '{endDate}'";
                dateFilterMR = $" mr.StartDate BETWEEN '{startDate}' AND '{endDate}'";
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                dateFilter = $" CreatedAt >= '{startDate}'";
                dateFilterMR = $" mr.StartDate >= '{startDate}'";
            }

            var clients = await _uow.OwnersRepository.CountToCard(dbName, null, dateFilter);
            var pets = await _uow.PatientsRepository.CountToCard(dbName, null, dateFilter);
            var appointment = await _uow.AppointmentRepository.CountToCard(dbName, null, dateFilter);
            var product = await _uow.ProductsRepository.CountToCard(dbName, null, dateFilter);
            var service = await _uow.ServicesRepository.CountToCard(dbName, null, dateFilter);
            var invoice = await _uow.OrdersRepository.CountInvoiceToCard(dbName, dateFilter);
            
            var weekClientAppointment = await _uow.AppointmentRepository.GetClientWeek(dbName);

            var revenueAppointmentMonth = await _uow.MedicalRecordsRepository.GetSalesDetail(dbName, $"{dateFilterMR}");
            var revenueOrderMonth = await _uow.OrdersRepository.SumDoubleWithQuery(dbName, "TotalPrice", $"{dateFilter} AND Status = '{statusPaid}' AND Type = 'Incomes'");
            var visitYearly = await _uow.MedicalRecordsRepository.GetVisitYearly(dbName, dateFilter);
            var ownerTotal = await _uow.OwnersRepository.GetOwnerChart(dbName, dateFilter);
            var patientTotal = await _uow.PatientsRepository.GetPatientChart(dbName, dateFilter);
            var patientTypes = await _uow.PatientsRepository.GetPatientTypeChart(dbName, defaultFilter);

            var medicalRecordsSales = await _uow.MedicalRecordsRepository.GetTotalMedicalSales(dbName, dateFilterMR);
            var orderSales = await _uow.OrdersRepository.GetTotalOrderSales(dbName, dateFilter);
            var topMedFrequency = await _uow.MedicalRecordsPrescriptionsRepository.GetMedsFrequency(dbName, dateFilter);

            var visitYearlyConverted = visitYearly.Select(item => new MonthlyDataChart()
            {
                Date = item.Date,
                Month = DateTime.ParseExact(item.Month, "MM", null).ToString("MMM"),
                Year = item.Year,
                Total = item.Total
            });

            //combine med and order sales
            var combinedSales = medicalRecordsSales.Concat(orderSales);
            var combinedSalesFinal = combinedSales
                .GroupBy(sale => new { sale.Date, sale.Month, sale.Year })
                .Select(group => new MonthlyDataChart
                {
                    Date = group.Key.Date,
                    Month = DateTime.ParseExact(group.Key.Month, "MM", null).ToString("MMM"),
                    Year = group.Key.Year,
                    Total = group.Sum(sale => sale.Total)
                })
                .OrderBy(sale => sale.Year)
                .ThenBy(sale => sale.Month)
                .ThenBy(sale => sale.Date)
                .ToList();

            var ownerTotalConverted = ownerTotal.Select(item => new MonthlyDataChart()
            {
                Date = item.Date,
                Month = DateTime.ParseExact(item.Month, "MM", null).ToString("MMM"),
                Year = item.Year,
                Type = item.Type,
                Total = item.Total
            });

            var patientTotalConverted = patientTotal.Select(item => new MonthlyDataChart()
            {
                Date = item.Date,
                Month = DateTime.ParseExact(item.Month, "MM", null).ToString("MMM"),
                Year = item.Year,
                Type = item.Type,
                Total = item.Total
            });

            var patientTypeConverted = patientTypes.Select(item => new MonthlyDataChart()
            {
                Date = item.Date,
                Month = DateTime.ParseExact(item.Month, "MM", null).ToString("MMM"),
                Year = item.Year,
                Type = item.Type,
                Total = item.Total
            });

            //var revenueAll = revenueOrderAll + revenueAppointmentAll;
            var revenueAppointmentTotal = (revenueAppointmentMonth.ProductsTotal + revenueAppointmentMonth.ServicesTotal) - revenueAppointmentMonth.TotalDiscount;
            var revenueMonth = revenueOrderMonth + revenueAppointmentTotal;

            var result = new DashboardResponse();
            clients.Percentage = FormatUtil.CountPercentageMonth(clients.Total, clients.TotalAll);
            result.Clients = clients;

            pets.Percentage = FormatUtil.CountPercentageMonth(pets.Total, pets.TotalAll);
            result.Pets = pets;

            appointment.Percentage = FormatUtil.CountPercentageMonth(appointment.Total, appointment.TotalAll);
            result.Appointments = appointment;

            product.Percentage = FormatUtil.CountPercentageMonth(product.Total, product.TotalAll);
            result.Products = product;

            service.Percentage = FormatUtil.CountPercentageMonth(service.Total, service.TotalAll);
            result.Services = service;

            invoice.Percentage = FormatUtil.CountPercentageMonth(invoice.Total, invoice.TotalAll);
            result.SalesInvoices = invoice;
            
            result.Revenues = new DoubleCardDashboard()
            {
                Total = revenueMonth,
                TotalAll = 0,
                Percentage = FormatUtil.CountDoublePercentageMonth(revenueMonth, 0)
            };

            //diganti ke revenue order
            result.RevenuesProducts = new DoubleCardDashboard()
            {
                Total = revenueOrderMonth,
                TotalAll = 0,
                Percentage = FormatUtil.CountDoublePercentageMonth(revenueOrderMonth, 0)
            };
            //diganti ke revenue medical
            result.RevenuesServices = new DoubleCardDashboard()
            {
                Total = revenueAppointmentTotal,
                TotalAll = 0,
                Percentage = FormatUtil.CountDoublePercentageMonth(revenueAppointmentTotal, 0)
            };

            //appointment activity & order activity
            var listActivities = new List<ActivityDashboard>();

            var latestAppointment = await _uow.AppointmentRepository.GetLastDetailList(dbName, null, 10);
            foreach (var item in latestAppointment.Data)
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
            result.ChartSalesGrowth = new List<ChartDataSeries>();
            result.ChartClientsGrowth = new List<ChartDataSeries>();
            result.ChartPatientType = new List<ChartDataSeries>();
            var visitChart = new ChartDataSeries()
            {
                Data = visitYearlyConverted,
                Name = "Visits"
            };
            var salesGrowthChart = new ChartDataSeries()
            {
                Data = combinedSalesFinal,
                Name = "SalesGrowth"
            };
            var clientGrowthChart = new ChartDataSeries()
            {
                Data = ownerTotalConverted,//belum
                Name = "ClientGrowth"
            };
            var patientGrowthChart = new ChartDataSeries()
            {
                Data = patientTotalConverted,//belum
                Name = "PatientGrowth"
            };
            var patientTypeChart = new ChartDataSeries()
            {
                Data = patientTypeConverted,
                Name = "Pets"
            };

            result.ChartClients.Add(visitChart);
            result.ChartSalesGrowth.Add(salesGrowthChart);
            result.ChartClientsGrowth.Add(clientGrowthChart);
            result.ChartClientsGrowth.Add(patientGrowthChart);
            result.ChartPatientType.Add(patientTypeChart);
            result.DateFilter = dateFilter;

            var heatmap = weekClientAppointment
                .GroupBy(r => r.W) // Group by day of the week
                .Select(g => new ChartHeatmapSeries
                {
                    Name = EnumConvertor.GetDayName(g.Key), // Convert numeric day to day name (e.g., 1 = Sunday)
                    Data = g.Select(r => new DataPoint { X = r.X, Y = r.Y })
                })
                .ToList();
            result.WeeklyClient = heatmap;
            result.FrequentDiagnoseMeds = topMedFrequency.ToList();

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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "CreateClinicsAsync", MethodType.Create, nameof(Clinics));
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreateClinicsAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Clinics), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "UpdateClinicsAsync", MethodType.Update, nameof(Clinics));
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdateClinicsAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Clinics), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "CreateAnimalAsync", MethodType.Create, nameof(Animals));
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreateAnimalAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Animals), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "UpdateAnimalAsync", MethodType.Update, nameof(Animals));
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdateAnimalAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Animals), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "DeleteAnimalAsync", MethodType.Delete, nameof(Animals));
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.DeleteAnimalAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Animals), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "CreateBreedAsync", MethodType.Create, nameof(Breeds));
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreateBreedAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Breeds), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "UpdateBreedAsync", MethodType.Update, nameof(Breeds));
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdateBreedAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Breeds), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "DeleteBreedAsync", MethodType.Delete, nameof(Breeds));
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.DeleteBreedAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Breeds), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "CreateDiagnoseAsync", MethodType.Create, nameof(Diagnoses));
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreateDiagnoseAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Diagnoses), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "UpdateDiagnoseAsync", MethodType.Update, nameof(Diagnoses));
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdateDiagnoseAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Diagnoses), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "DeleteDiagnoseAsync", MethodType.Delete, nameof(Diagnoses));
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.DeleteDiagnoseAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Diagnoses), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "CreatePaymentMethodAsync", MethodType.Create, nameof(PaymentMethod));
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreatePaymentMethodAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(PaymentMethod), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "UpdatePaymentMethodAsync", MethodType.Update, nameof(PaymentMethod));
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdatePaymentMethodAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(PaymentMethod), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "DeletePaymentMethodAsync", MethodType.Delete, nameof(PaymentMethod));
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.DeletePaymentMethodAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(PaymentMethod), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "CreatePrescriptionFrequentsAsync", MethodType.Create, nameof(PrescriptionFrequents));
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreatePrescriptionFrequentsAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(PrescriptionFrequents), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "UpdatePrescriptionFrequentsAsync", MethodType.Update, nameof(PrescriptionFrequents));
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdatePrescriptionFrequentsAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(PrescriptionFrequents), ex);
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

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _uow.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "DeletePrescriptionFrequentsAsync", MethodType.Delete, nameof(PrescriptionFrequents));
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.DeletePrescriptionFrequentsAsync";
                await _uow.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(PrescriptionFrequents), ex);
                throw;
            }
        }
        #endregion

        #region Clinic Config
        public async Task<IEnumerable<ClinicConfig>> ReadAllClinicConfigAsync(string dbName)
        {
            try
            {
                var filePath = $"{Directory.GetCurrentDirectory()}/wwwroot/DataInsert/initData.json";
                var existingData = await _uow.ClinicConfigRepository.GetAll(dbName);
                bool isSynced = true;

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var deserializedObjects = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (deserializedObjects != null && deserializedObjects.ContainsKey("ClinicConfig"))
                    {
                        var clinicConfigJson = deserializedObjects["ClinicConfig"].ToString();
                        var dataTemplate = JsonConvert.DeserializeObject<IEnumerable<ClinicConfig>>(clinicConfigJson);

                        foreach (var config in dataTemplate)
                        {
                            var existingConfig = existingData.FirstOrDefault(x => x.Key == config.Key);
                            if (existingConfig == null)
                            {
                                // If the config does not exist, add it to the database
                                var newConfig = new ClinicConfig
                                {
                                    Key = config.Key,
                                    Value = config.Value,
                                };
                                FormatUtil.SetIsActive<ClinicConfig>(newConfig, true);
                                FormatUtil.SetDateBaseEntity<ClinicConfig>(newConfig);

                                await _uow.ClinicConfigRepository.AddConfig(dbName, newConfig);
                                isSynced = false;
                            }
                            //else if (existingConfig.Value != config.Value)
                            //{
                            //    // If the config exists but the value is different, update it
                            //    existingConfig.Value = config.Value;
                            //    await _uow.ClinicConfigRepository.UpdateConfig(dbName, existingConfig);
                            //    isSynced = false;
                            //}
                        }
                    }
                }

                if (isSynced)
                {
                    return existingData;
                }
                else
                {
                    return await _uow.ClinicConfigRepository.GetAll(dbName);
                }
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadClinicConfigAsync";
                throw;
            }
        }
        public async Task<ClinicConfig> ReadClinicConfigAsync(string dbName, string key)
        {
            try
            {
                var data = await _uow.ClinicConfigRepository.GetConfigByKey(dbName, key);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.ReadClinicConfigAsync";
                throw;
            }
        }
        public async Task<ClinicConfig> UpdateClinicConfigAsync(string key, string newValue, string dbName)
        {
            try
            {

                ClinicConfig checkedEntity = await _uow.ClinicConfigRepository.GetConfigByKey(dbName, key);
                if (checkedEntity == null)
                {
                    throw new Exception($"No ClinicConfig found with Key = {key}");
                }
                checkedEntity.Value = newValue;
                await _uow.ClinicConfigRepository.UpdateConfig(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.UpdateClinicConfigAsync";
                throw;
            }
        }
        #endregion
    }
}
