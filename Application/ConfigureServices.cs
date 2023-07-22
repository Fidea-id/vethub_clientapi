using Application.Services.Contracts;
using Application.Services.Implementations;
using Application.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericService<,,,>), typeof(GenericService<,,,>));

            services.AddScoped<IMasterService, MasterService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IOwnersService, OwnersService>();
            services.AddScoped<IPatientsService, PatientsService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IProductsService, ProductsService>();
            services.AddScoped<IServicesService, ServicesService>();
            services.AddScoped<IAdditionalDataService, AdditionalDataService>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = false,
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = JwtUtil.Audience,
                    ValidIssuer = JwtUtil.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtUtil.Key))
                };
            });

            return services;
        }
    }
}
