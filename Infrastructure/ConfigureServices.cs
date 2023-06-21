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
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IServicesRepository, ServicesRepository>();

            //UOW
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //EmailSender
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddFluentEmail("SenderEmailAddress", "SenderName").AddRazorRenderer()
                .AddSmtpSender(new SmtpClient("smtp.gmail.com", 587)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("SenderEmailAddress", "SenderEmailPassword"),
                    EnableSsl = true
                });
            return services;
        }
    }
}
