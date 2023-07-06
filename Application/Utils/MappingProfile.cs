using AutoMapper;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;

namespace Application.Utils
{
    public static class Mapping
    {
        private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                // This line ensures that internal properties are also mapped over.
                cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
                cfg.AddProfile<MappingProfile>();
            });
            var mapper = config.CreateMapper();
            return mapper;
        });

        public static IMapper Mapper => Lazy.Value;
    }

    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            //model to response
            CreateMap<Domain.Entities.Models.Clients.Profile, UserProfileResponse>();

            //request to model
            CreateMap<OwnersRequest, Owners>();
            CreateMap<OwnersAddRequest, Owners>();
            CreateMap<PetsAddRequest, Patients>();
            CreateMap<ProfileRequest, Domain.Entities.Models.Clients.Profile>();
            CreateMap<AppointmentsRequest, Appointments>();
            CreateMap<ServicesRequest, Domain.Entities.Models.Clients.Services>();

            //DTO to request


            //DTO to model
        }
    }
}
