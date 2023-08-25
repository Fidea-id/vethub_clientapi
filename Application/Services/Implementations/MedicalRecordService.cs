using Application.Services.Contracts;
using Application.Utils;
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

namespace Application.Services.Implementations
{
    public class MedicalRecordService : GenericService<MedicalRecords, MedicalRecordsRequest, MedicalRecordsResponse, MedicalRecordsFilter>, IMedicalRecordService
    {
        public MedicalRecordService(IUnitOfWork unitOfWork, IGenericRepository<MedicalRecords, MedicalRecordsFilter> repository)
        : base(unitOfWork, repository)
        { }

        public async Task<MedicalRecordsDetailResponse> PostAllMedicalRecords(MedicalRecordsDetailRequest request, string dbName)
        {
            throw new NotImplementedException();
        }

        public async Task<MedicalRecordsNotesResponse> PostMedicalRecordsNotes(MedicalRecordsNotesRequest request, string email, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<MedicalRecordsNotes>(request);
                FormatUtil.SetIsActive<MedicalRecordsNotes>(entity, true);
                FormatUtil.SetDateBaseEntity<MedicalRecordsNotes>(entity);
                var staff = await _unitOfWork.ProfileRepository.GetByEmail(dbName, email);
                entity.StaffId = staff.Id;
                var newId = await _unitOfWork.MedicalRecordsNotesRepository.Add(dbName, entity);

                var response = new MedicalRecordsNotesResponse
                {
                    Id = newId,
                    MedicalRecordsId = request.MedicalRecordsId,
                    StaffId = request.MedicalRecordsId,
                    Title = request.Title,
                    Type = request.Type,
                    Value = request.Value
                };
                return response;
            }
            catch (Exception ex)
            {
                ex.Source = $"AdditionalDataService.CreateAnimalAsync";
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

    }
}
