using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src =>
                    src.UserRoles != null && src.UserRoles.Any() && src.UserRoles.First().Role != null
                        ? src.UserRoles.First().Role!.Name.ToString()
                        : null));
        }
    }
}