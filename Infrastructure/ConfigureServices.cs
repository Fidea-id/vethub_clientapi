using Domain.Interfaces;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Mail;

namespace Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddTransient<IDBFactory, DBFactory>();
            //Repository
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped<IGenerateTableRepository, GenerateTableRepository>();
            services.AddScoped<IOwnersRepository, OwnersRepository>();
            services.AddScoped<IPatientsRepository, PatientsRepository>();
            services.AddScoped<IProductsRepository, ProductsRepository>();
            services.AddScoped<IProductBundlesRepository, ProductBundlesRepository>();
            services.AddScoped<IProductCategoriesRepository, ProductCategoriesRepository>();
            services.AddScoped<IProductDiscountsRepository, ProductDiscountsRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IServicesRepository, ServicesRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IAnimalRepository, AnimalRepository>();
            services.AddScoped<IBreedRepository, BreedRepository>();
            services.AddScoped<IPatientsStatisticRepository, PatientsStatisticRepository>();
            services.AddScoped<IOrdersRepository, OrdersRepository>();
            services.AddScoped<IOrdersDetailRepository, OrdersDetailRepository>();
            services.AddScoped<IOrdersPaymentRepository, OrdersPaymentRepository>();
            services.AddScoped<IClinicRepository, ClinicRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            services.AddScoped<IMedicalRecordsRepository, MedicalRecordsRepository>();
            services.AddScoped<IMedicalRecordsPrescriptionsRepository, MedicalRecordsPrescriptionsRepository>();
            services.AddScoped<IMedicalRecordsNotesRepository, MedicalRecordsNotesRepository>();
            services.AddScoped<IMedicalRecordsDiagnosesRepository, MedicalRecordsDiagnosesRepository>();
            services.AddScoped<IPrescriptionFrequentsRepository, PrescriptionFrequentsRepository>();
            services.AddScoped<INotificationsRepository, NotificationsRepository>();
            services.AddScoped<IProductStockHistoricalRepository, ProductStockHistoricalRepository>();
            services.AddScoped<IOpnamesRepository, OpnamesRepository>();
            services.AddScoped<IOpnamePatientsRepository, OpnamePatientsRepository>();
            services.AddScoped<IEventLogRepository, EventLogRepository>();

            //UOW
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //EmailSender
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddFluentEmail("no-reply@vethub.id", "Vethub").AddRazorRenderer()
                .AddSmtpSender(new SmtpClient("sandbox.smtp.mailtrap.io", 587)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("c1e821993e0969", "b07a3bfd12ed78"),
                    EnableSsl = true
                });
            return services;
        }
    }
}
