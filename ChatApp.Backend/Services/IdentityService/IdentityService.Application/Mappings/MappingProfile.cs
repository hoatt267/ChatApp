using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponseDto>();
        }
    }
}