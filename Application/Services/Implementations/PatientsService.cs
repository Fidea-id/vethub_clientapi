﻿using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;

namespace Application.Services.Implementations
{
    public class PatientsService : GenericService<Patients, PatientsRequest, Patients, PatientsFilter>, IPatientsService
    {
        public PatientsService(IUnitOfWork unitOfWork, IGenericRepository<Patients, PatientsFilter> repository)
        : base(unitOfWork, repository)
        { }

        public async Task<IEnumerable<PatientsListResponse>> ReadPatientsList(PatientsFilter filter, string dbName)
        {
            return await _unitOfWork.PatientsRepository.GetPatientsList(dbName, filter);
        }
    }
}
