using Application.Services.Contracts;
using Application.Services.Implementations;
using Application.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Security.Claims;
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
            services.AddScoped<IOrdersService, OrdersService>();
            services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IOpnameService, OpnameService>();
            services.AddScoped<IChartOfAccountsService, ChartOfAccountsService>();
            services.AddScoped<IFinancialService, FinancialService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ISessionVersionValidator, MasterSessionVersionValidator>();
            services.AddHttpClient();

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
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userId = context.Principal?.FindFirstValue("Id");
                        var sessionVersion = context.Principal?.FindFirstValue("sv");
                        if (!int.TryParse(userId, out var parsedUserId) || !int.TryParse(sessionVersion, out var parsedSessionVersion))
                        {
                            context.Fail("Session version claim is missing or invalid.");
                            return;
                        }

                        var validator = context.HttpContext.RequestServices.GetRequiredService<ISessionVersionValidator>();
                        try
                        {
                            if (!await validator.IsValidAsync(parsedUserId, parsedSessionVersion, context.HttpContext.RequestAborted))
                            {
                                context.Fail("Session has been revoked.");
                            }
                        }
                        catch
                        {
                            context.Fail("Unable to validate session.");
                        }
                    }
                };
            });

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = ConfigurationOptions.Parse("103.217.145.102:6379,password=BFvnAqcptn");
                config.ResolveDns = true;
                return ConnectionMultiplexer.Connect(config);
            });

            services.AddScoped<ICacheService, RedisCacheService>();

            return services;
        }
    }
}
