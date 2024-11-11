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
            services.AddScoped<IClinicConfigRepository, ClinicConfigRepository>();
            services.AddScoped<IAppointmentsTypeRepository, AppointmentsTypeRepository>();

            //UOW
            services.AddScoped<IUnitOfWork, UnitOfWork>();

			//EmailSender
			services.AddScoped<IEmailSender, EmailSender>();
			services.AddFluentEmail("info@vethub.id", "VetHub").AddRazorRenderer()
				.AddSmtpSender(new SmtpClient("live.smtp.mailtrap.io", 587)
				{
					UseDefaultCredentials = false,
					Credentials = new NetworkCredential("api", "0f874589507aa1da9c4f8da918698233"),
					EnableSsl = true
				});
			return services;
        }
    }
}
