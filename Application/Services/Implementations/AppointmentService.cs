using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class AppointmentService : GenericService<Appointments, AppointmentsRequest, Appointments, AppointmentsFilter>, IAppointmentService
    {
        public AppointmentService(IUnitOfWork unitOfWork, IGenericRepository<Appointments, AppointmentsFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}
