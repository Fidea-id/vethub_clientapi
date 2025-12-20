using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;

namespace Application.Services.Implementations
{
    public class OwnersService : GenericService<Owners, OwnersRequest, Owners, OwnersFilter>, IOwnersService
    {
        private ILogger<ProductsService> _logger;
        public OwnersService(IUnitOfWork unitOfWork, IGenericRepository<Owners, OwnersFilter> repository, ILoggerFactory loggerFactory, ICurrentUserService currentUser)
        : base(unitOfWork, repository, currentUser)
        {
            _logger = loggerFactory.CreateLogger<ProductsService>();
        }

        public async Task<ResponseUploadBulk> CheckOwnersPatientsAsBulk(IEnumerable<BulkOwnerPatient> request, string dbName, string globalId)
        {
            var checkedGroups = new CheckValidDTO();
            checkedGroups.ValidationMessage = new List<string>();
            var counter = 0;
            try
            {
                _logger.LogInformation("Validating Owner-Patient data: " + JsonConvert.SerializeObject(request));
                checkedGroups = await _unitOfWork.OwnersRepository.CheckOwnerPatientValidList(request, dbName);
                _logger.LogInformation("Validation result: " + JsonConvert.SerializeObject(checkedGroups));

                if (checkedGroups.Status == 200)
                {
                    foreach (var item in request)
                    {
                        try
                        {
                            // Trim all string properties
                            FormatUtil.TrimObjectProperties(item);

                            // Check if Owner exists
                            var existingOwner = await _unitOfWork.OwnersRepository.WhereFirstQuery(
                                dbName, $"LOWER(Name) = '{item.ownerName.ToLower()}' AND LOWER(PhoneNumber) = '{item.ownerPhone.ToLower()}'");

                            int ownerId;
                            if (existingOwner != null)
                            {
                                ownerId = existingOwner.Id;
                                // Check if Patient exists under that Owner
                                var existingPatient = await _unitOfWork.PatientsRepository.WhereFirstQuery(
                                    dbName, $"LOWER(Name) = '{item.patientName.ToLower()}' AND LOWER(Species) = '{item.patientSpecies.ToLower()}' AND LOWER(Breed) = '{item.patientBreed.ToLower()}' AND OwnersId = {ownerId}");
                                if (existingPatient != null)
                                {
                                    checkedGroups.Message = $"Row {item.row}: Patient already exists under this Owner.";
                                }
                            }

                            DateTime? dateOfBirth = null;
                            if (!string.IsNullOrEmpty(item.patienDOB))
                            {
                                if (DateTime.TryParseExact(item.patienDOB,
                                    new[] { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy" },
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                                {
                                    dateOfBirth = parsedDate;
                                }
                                else
                                {
                                    checkedGroups.Message = $"Row {item.row}: Invalid date format for Patient DOB. Use 'yyyy-MM-dd' or 'MM/dd/yyyy'.";
                                }
                            }

                            counter++;
                        }
                        catch (Exception e)
                        {
                            checkedGroups.ValidationMessage.Add($"Row {item.row}: cannot be saved. " + e.Message);
                            checkedGroups.Message = "Fail";
                        }
                    }

                    if (checkedGroups.Message == "Fail")
                    {
                        checkedGroups.Message = $"Success: {counter} records added, some failed.";
                    }
                    else
                    {
                        checkedGroups.Message = $"Success: {counter} records added.";
                    }
                }
            }
            catch (Exception e)
            {
                checkedGroups.ValidationMessage.Add("Error: " + e.Message);
                checkedGroups.Message = "Fail";
                checkedGroups.Status = 500;  // Set to 500 for unexpected server error
            }

            return new ResponseUploadBulk()
            {
                validationMessage = checkedGroups.ValidationMessage,
                message = checkedGroups.Message,
                status = checkedGroups.Status
            };
        }
        public async Task<ResponseUploadBulk> AddOwnersPatientsAsBulk(IEnumerable<BulkOwnerPatient> request, string dbName, string globalId)
        {
            var checkedGroups = new CheckValidDTO();
            checkedGroups.ValidationMessage = new List<string>();
            var counter = 0;
            try
            {
                _logger.LogInformation("Validating Owner-Patient data: " + JsonConvert.SerializeObject(request));
                checkedGroups = await _unitOfWork.OwnersRepository.CheckOwnerPatientValidList(request, dbName);
                _logger.LogInformation("Validation result: " + JsonConvert.SerializeObject(checkedGroups));

                if (checkedGroups.Status == 200)
                {
                    var listAnimals = new List<Animals>();
                    var listBreeds = new List<Breeds>();
                    foreach (var item in request)
                    {
                        try
                        {
                            // Trim all string properties
                            FormatUtil.TrimObjectProperties(item);

                            // Check if Owner exists
                            var existingOwner = await _unitOfWork.OwnersRepository.WhereFirstQuery(
                                dbName, $"LOWER(Name) = '{item.ownerName.ToLower()}' AND LOWER(PhoneNumber) = '{item.ownerPhone.ToLower()}'");

                            int ownerId;
                            if (existingOwner != null)
                            {
                                ownerId = existingOwner.Id;
                            }
                            else
                            {
                                // Add new Owner
                                var newOwner = new Owners()
                                {
                                    Title = item.ownerTitle ?? "",
                                    Name = item.ownerName,
                                    Email = item.ownerEmail ?? "",
                                    PhoneNumber = item.ownerPhone,
                                    Address = item.ownerAddress ?? ""
                                };

                                FormatUtil.SetIsActive<Owners>(newOwner, true);
                                FormatUtil.SetDateBaseEntity<Owners>(newOwner);
                                ownerId = await _unitOfWork.OwnersRepository.Add(dbName, newOwner);

                                // Add event log for owner creation
                                var currentUserId = await _currentUser.UserId;
                                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, ownerId, "AddOwnersPatientsAsBulk", MethodType.Create, nameof(Owners));
                            }

                            // Check if Patient exists under that Owner
                            var existingPatient = await _unitOfWork.PatientsRepository.WhereFirstQuery(
                                dbName, $"LOWER(Name) = '{item.patientName.ToLower()}' AND OwnersId = {ownerId}");

                            if (existingPatient == null)
                            {
                                DateTime? dateOfBirth = null;
                                if (!string.IsNullOrEmpty(item.patienDOB))
                                {
                                    if (DateTime.TryParseExact(item.patienDOB,
                                        new[] { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy" },
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                                    {
                                        dateOfBirth = parsedDate;
                                    }
                                    else
                                    {
                                        checkedGroups.Message = $"Row {item.row}: Invalid date format for Patient DOB. Use 'yyyy-MM-dd' or 'MM/dd/yyyy'.";
                                    }
                                }

                                // Check if species exists in the local list first
                                var existingSpecies = listAnimals.FirstOrDefault(a => a.Name == item.patientSpecies);

                                if (existingSpecies == null)
                                {
                                    existingSpecies = await _unitOfWork.AnimalRepository.WhereFirstQuery(dbName, $"Name = '{item.patientSpecies}'");

                                    if (existingSpecies == null)
                                    {
                                        var newSpecies = new Animals { Name = item.patientSpecies };
                                        var speciesId = await _unitOfWork.AnimalRepository.Add(dbName, newSpecies);
                                        existingSpecies = new Animals { Id = speciesId, Name = item.patientSpecies };
                                    }

                                    listAnimals.Add(existingSpecies); // Add to local cache
                                }

                                // Check if breed exists in the local list first
                                var existingBreed = listBreeds.FirstOrDefault(b => b.Name == item.patientBreed && b.AnimalsId == existingSpecies.Id);

                                if (existingBreed == null)
                                {
                                    existingBreed = await _unitOfWork.BreedRepository.WhereFirstQuery(dbName, $"Name = '{item.patientBreed}' AND AnimalsId = {existingSpecies.Id}");

                                    if (existingBreed == null)
                                    {
                                        var newBreed = new Breeds { Name = item.patientBreed, AnimalsId = existingSpecies.Id };
                                        var breedId = await _unitOfWork.BreedRepository.Add(dbName, newBreed);
                                        existingBreed = new Breeds { Id = breedId, Name = item.patientBreed, AnimalsId = existingSpecies.Id };
                                    }

                                    listBreeds.Add(existingBreed); // Add to local cache
                                }

                                // Add new Patient
                                var newPatient = new Patients()
                                {
                                    OwnersId = ownerId,
                                    Name = item.patientName,
                                    Species = item.patientSpecies,
                                    Breed = item.patientBreed,
                                    Gender = item.patientGender,
                                    Color = item.patienColor ?? "",
                                    DateOfBirth = dateOfBirth ?? DateTime.MinValue,
                                    IsAlive = item.isAlive ?? true,
                                    Vaccinated = item.isVaccinated ?? false
                                };

                                FormatUtil.SetIsActive<Patients>(newPatient, true);
                                FormatUtil.SetDateBaseEntity<Patients>(newPatient);
                                var newPatientId = await _unitOfWork.PatientsRepository.Add(dbName, newPatient);

                                // Add event log for patient creation
                                var currentUserId = await _currentUser.UserId;
                                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newPatientId, "AddOwnersPatientsAsBulk", MethodType.Create, nameof(Patients));

                                counter++;
                            }
                            else
                            {
                                _logger.LogWarning($"Row {item.row}: Patient already exists under this Owner.");
                            }
                        }
                        catch (Exception e)
                        {
                            checkedGroups.ValidationMessage.Add($"Row {item.row}: cannot be saved. " + e.Message);
                            checkedGroups.Message = "Fail";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                checkedGroups.ValidationMessage.Add("Error: " + e.Message);
                checkedGroups.Message = "Fail";
                checkedGroups.Status = 500;  // Set to 500 for unexpected server error
            }

            return new ResponseUploadBulk()
            {
                validationMessage = checkedGroups.ValidationMessage,
                message = checkedGroups.Message,
                status = checkedGroups.Status
            };
        }

        public async Task<Owners> CreateOwnersPetsAsync(OwnersPetsRequest request, string dbName)
        {
            try
            {
                //map all data
                var ownerAdd = Mapping.Mapper.Map<Owners>(request.OwnersData);
                var petsAdd = Mapping.Mapper.Map<IEnumerable<Patients>>(request.PetsData);
                //validate unique data
                if (!string.IsNullOrWhiteSpace(ownerAdd.Email))
                {
                    var checkOwners = await _repository.AnyQuery(
                        dbName,
                        $"Email = '{ownerAdd.Email}' AND IsActive = 1"
                    );

                    if (checkOwners)
                        throw new Exception("Owners email already added");
                }

                //create owner and return id owner
                FormatUtil.TrimObjectProperties(ownerAdd);
                FormatUtil.SetIsActive<Owners>(ownerAdd, true);
                FormatUtil.SetDateBaseEntity<Owners>(ownerAdd);
                var ownerId = await _repository.Add(dbName, ownerAdd);

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, ownerId, "CreateOwnersPetsAsync", MethodType.Create, nameof(Owners));

                //create patients range
                foreach (var pet in petsAdd)
                {
                    FormatUtil.TrimObjectProperties(pet);
                    FormatUtil.SetIsActive<Patients>(pet, true);
                    FormatUtil.SetDateBaseEntity<Patients>(pet);
                    pet.OwnersId = ownerId;
                }
                await _unitOfWork.PatientsRepository.AddRange(dbName, petsAdd);

                //add event log
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, ownerId, "CreateOwnersPetsAsync", MethodType.Create, nameof(Patients), JsonConvert.SerializeObject(petsAdd));
                //return owner data
                ownerAdd.Id = ownerId;
                return ownerAdd;
            }
            catch (Exception ex)
            {
                ex.Source = "OwnersService.CreateOwnersPetsAsync";
                await _unitOfWork.EventLogRepository.AddErrorEventLogByParams(dbName, nameof(Patients), ex);
                throw;
            }
        }

        public async Task DeleteOwnerAsync(int ownerId, string dbName)
        {
            //get patient data
            var patients = await _unitOfWork.PatientsRepository.GetPatientsByOwner(dbName, ownerId);
            //get appointment data
            var appointments = await _unitOfWork.AppointmentRepository.GetBookingHistoryOwner(dbName, ownerId);

            //delete owner data
            await _repository.Remove(dbName, ownerId);

            //delete patients data
            if (patients.Count() > 0)
            {
                foreach (var patient in patients)
                {
                    await _unitOfWork.PatientsRepository.Remove(dbName, patient.Id);
                }
            }

            //delete appointment data
            if (appointments.Count() > 0)
            {
                foreach (var appointment in appointments)
                {
                    await _unitOfWork.AppointmentRepository.Remove(dbName, appointment.AppointmentId);
                }
            }
        }

        public async Task<OwnerStatistic> GetOwnerStatisticAsync(int ownerId, string dbName)
        {
            try
            {
                var result = new OwnerStatistic();
                //get booking data
                var bookings = await _unitOfWork.AppointmentRepository.GetBookingHistoryOwner(dbName, ownerId);
                result.TotalBooking = bookings.Count();
                result.TotalPayment = Convert.ToInt64(bookings.Where(x => x.StatusPayment == "Paid").Sum(x => x.TotalPrice));
                result.LastVisit = bookings.OrderByDescending(x => x.DateAppointment).Select(x => x.DateAppointment).First();
                return result;
            }
            catch (Exception ex)
            {
                ex.Source = "OwnersService.GetOwnerStatisticAsync";
                throw;
            }
        }
    }
}
