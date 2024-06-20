using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Newtonsoft.Json;

namespace Application.Services.Implementations
{
    public class OwnersService : GenericService<Owners, OwnersRequest, Owners, OwnersFilter>, IOwnersService
    {
        public OwnersService(IUnitOfWork unitOfWork, IGenericRepository<Owners, OwnersFilter> repository, ICurrentUserService currentUser)
        : base(unitOfWork, repository, currentUser)
        { }

        public async Task<Owners> CreateOwnersPetsAsync(OwnersPetsRequest request, string dbName)
        {
            try
            {
                //map all data
                var ownerAdd = Mapping.Mapper.Map<Owners>(request.OwnersData);
                var petsAdd = Mapping.Mapper.Map<IEnumerable<Patients>>(request.PetsData);
                //validate unique data
                var checkOwners = await _repository.AnyQuery(dbName, $"Email = '{ownerAdd.Email}' AND IsActive = 1");
                if (checkOwners) throw new Exception("Owners email already added");

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
                throw;
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
