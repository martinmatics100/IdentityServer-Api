
using AutoMapper;
using SchoolPilot_IdentityServer.Data.Contracts;
using SchoolPilot_IdentityServer.Data.Entities;

namespace SchoolPilot_IdentityServer.Infrastructure.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserResponse>();
            CreateMap<ApplicationUser, CurrentUserResponse>();
            CreateMap<UserRegisterRequest, ApplicationUser>();

        }
    }
}
