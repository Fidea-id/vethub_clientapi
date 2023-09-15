using Application.Services.Contracts;
using Domain.Interfaces.Clients;

namespace Application.Services.Implementations
{
    public class CronService : ICronService
    {
        private IUnitOfWork _uow;

        public CronService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<string> CheckAppointmentsStatusDaily(string dbName)
        {
            var msg = "";
            try
            {
                var all = await _uow.AppointmentRepository.WhereQuery(dbName, "");
            }
            catch (Exception)
            {
                throw;
            }

            return msg;
        }
    }
}
