using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;

namespace Application.Services.Implementations
{
    public class OpnameService : GenericService<Opnames, OpnamesRequest, Opnames, OpnamesFilter>, IOpnameService
    {
        public OpnameService(IUnitOfWork unitOfWork, IGenericRepository<Opnames, OpnamesFilter> repository, ICurrentUserService currentUser)
        : base(unitOfWork, repository, currentUser)
        { }

        public async Task<OpnamePatients> CreateOpnamePatientsAsync(OpnamePatientsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<OpnamePatients>(request);
                FormatUtil.SetIsActive<OpnamePatients>(entity, true);
                FormatUtil.SetDateBaseEntity<OpnamePatients>(entity);

                //check opname data first
                var checkedOpname = await _unitOfWork.OpnamesRepository.GetById(dbName, request.OpnameId);
                if (checkedOpname == null) throw new Exception($"Opname not found");

                //check medicalrecord data first
                var checkedMedicalRecord = await _unitOfWork.MedicalRecordsRepository.GetById(dbName, request.MedicalRecordId);
                if (checkedMedicalRecord == null) throw new Exception($"MedicalRecord not found");

                var newId = await _unitOfWork.OpnamePatientsRepository.Add(dbName, entity);
                entity.Id = newId;

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, newId, "CreateOpnamePatientsAsync", MethodType.Create, nameof(OpnamePatients));
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"OpnameService.CreateOpnamePatientsAsync";
                throw;
            }
        }

        public async Task DeleteOpnamePatientsAsync(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _unitOfWork.OpnamePatientsRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _unitOfWork.OpnamePatientsRepository.Remove(dbName, id);

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "DeleteOpnamePatientsAsync", MethodType.Delete, nameof(OpnamePatients));
            }
            catch (Exception ex)
            {
                ex.Source = $"OpnameService.DeleteOpnamePatientsAsync";
                throw;
            }
        }

        public async Task<DataResultDTO<OpnamePatients>> ReadOpnamePatientsAllAsync(OpnamePatientsFilter filter, string dbName)
        {
            try
            {
                var data = await _unitOfWork.OpnamePatientsRepository.GetByFilter(dbName, filter);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"OpnameService.ReadOpnamePatientsAllAsync";
                throw;
            }
        }

        public async Task<OpnamePatients> ReadOpnamePatientsByIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _unitOfWork.OpnamePatientsRepository.GetById(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"OpnameService.ReadOpnamePatientsByIdAsync";
                throw;
            }
        }

        public async Task<DataResultDTO<OpnamePatients>> ReadOpnamePatientsByMedIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _unitOfWork.OpnamePatientsRepository.GetByMedId(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"OpnameService.ReadOpnamePatientsByMedIdAsync";
                throw;
            }
        }

        public async Task<DataResultDTO<OpnamePatients>> ReadOpnamePatientsByOpnameIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _unitOfWork.OpnamePatientsRepository.GetByOpnameId(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"OpnameService.ReadOpnamePatientsByOpnameIdAsync";
                throw;
            }
        }

        public async Task<DataResultDTO<OpnamePatientsDetailResponse>> ReadOpnamePatientsDetailAsync(OpnamePatientsFilter filter, string dbName)
        {
            try
            {
                var dataResult = new List<OpnamePatientsDetailResponse>();
                var data = await _unitOfWork.OpnamePatientsRepository.GetByFilter(dbName, filter);
                foreach(var item in data.Data)
                {
                    var opname = await _unitOfWork.OpnamesRepository.GetById(dbName, item.OpnameId);
                    var medical = await _unitOfWork.MedicalRecordsRepository.GetDetailById(dbName, item.MedicalRecordId, null);

                    var itemResult = new OpnamePatientsDetailResponse();
                    itemResult.Id = item.Id;
                    itemResult.OpnameId = item.OpnameId;
                    itemResult.MedicalRecordId = item.MedicalRecordId;
                    itemResult.Status = item.Status;
                    itemResult.StartTime = item.StartTime;
                    itemResult.EndTime = item.EndTime;
                    itemResult.EstimatedDays = item.EstimateDays;
                    itemResult.Price = item.Price;
                    itemResult.TotalPrice = item.TotalPrice;
                    itemResult.OpnameName = opname.Name;
                    itemResult.PatientName = medical.Patients.Name;
                    itemResult.PatientId = medical.Patients.Id;
                    dataResult.Add(itemResult);
                }
                var result = new DataResultDTO<OpnamePatientsDetailResponse>
                {
                    Data = dataResult,
                    TotalData = dataResult.Count
                };
                return result;
            }
            catch (Exception ex)
            {
                ex.Source = $"OpnameService.ReadOpnamePatientsDetailAsync";
                throw;
            }
        }

        public async Task<OpnamePatients> UpdateOpnamePatientsAsync(int id, OpnamePatientsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<OpnamePatients>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<OpnamePatients>(entity, true);

                OpnamePatients checkedEntity = await _unitOfWork.OpnamePatientsRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<OpnamePatients, OpnamePatients>(entity, checkedEntity);
                FormatUtil.SetIsActive<OpnamePatients>(checkedEntity, true);
                await _unitOfWork.OpnamePatientsRepository.Update(dbName, checkedEntity);

                //add event log
                var currentUserId = await _currentUser.UserId;
                await _unitOfWork.EventLogRepository.AddEventLogByParams(dbName, currentUserId, id, "UpdateOpnamePatientsAsync", MethodType.Update, nameof(OpnamePatients));
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"OpnameService.UpdateOpnamePatientsAsync";
                throw;
            }
        }
    }
}
