using Domain.Entities.DTOs;
using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IAdditionalDataService
    {
        //Clinics
        Task<Clinics> CreateClinicsAsync(ClinicsRequest entity, string dbName);
        Task<Clinics> ReadClinicsAsync(string dbName);
        Task<Clinics> UpdateClinicsAsync(int id, ClinicsRequest entity, string dbName);

        //animal
        Task<Animals> CreateAnimalAsync(AnimalsRequest entity, string dbName);
        Task<DataResultDTO<Animals>> ReadAnimalAllAsync(NameBaseEntityFilter filter, string dbName);
        Task<Animals> ReadAnimalByIdAsync(int id, string dbName);
        Task<Animals> UpdateAnimalAsync(int id, AnimalsRequest entity, string dbName);
        Task DeleteAnimalAsync(int id, string dbName);

        //breed
        Task<Breeds> CreateBreedAsync(BreedsRequest entity, string dbName);
        Task<DataResultDTO<BreedAnimalResponse>> ReadBreedAllAsync(NameBaseEntityFilter filter, string dbName);
        Task<BreedAnimalResponse> ReadBreedByIdAsync(int id, string dbName);
        Task<IEnumerable<BreedAnimalResponse>> ReadBreedByIdAnimalAsync(int idAnimal, string dbName);
        Task<Breeds> UpdateBreedAsync(int id, BreedsRequest entity, string dbName);
        Task DeleteBreedAsync(int id, string dbName);

        //Diagnoses
        Task<Diagnoses> CreateDiagnoseAsync(DiagnosesRequest entity, string dbName);
        Task<DataResultDTO<Diagnoses>> ReadDiagnoseAllAsync(NameBaseEntityFilter filter, string dbName);
        Task<Diagnoses> ReadDiagnoseByIdAsync(int id, string dbName);
        Task<Diagnoses> UpdateDiagnoseAsync(int id, DiagnosesRequest entity, string dbName);
        Task DeleteDiagnoseAsync(int id, string dbName);

        //PaymentMethod
        Task<PaymentMethod> CreatePaymentMethodAsync(PaymentMethodRequest entity, string dbName);
        Task<DataResultDTO<PaymentMethod>> ReadPaymentMethodAllAsync(NameBaseEntityFilter filter, string dbName);
        Task<PaymentMethod> ReadPaymentMethodByIdAsync(int id, string dbName);
        Task<PaymentMethod> UpdatePaymentMethodAsync(int id, PaymentMethodRequest entity, string dbName);
        Task DeletePaymentMethodAsync(int id, string dbName);

        //PrescriptionFrequents
        Task<PrescriptionFrequents> CreatePrescriptionFrequentsAsync(PrescriptionFrequentsRequest entity, string dbName);
        Task<DataResultDTO<PrescriptionFrequents>> ReadPrescriptionFrequentsAllAsync(PrescriptionFrequentsFilter filter, string dbName);
        Task<PrescriptionFrequents> UpdatePrescriptionFrequentsAsync(int id, PrescriptionFrequentsRequest entity, string dbName);
        Task DeletePrescriptionFrequentsAsync(int id, string dbName);
    }
}
