using AutoMapper;
using UserService.Application.DTOs.Response;
using UserService.Domain.Entities;

namespace UserService.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserProfile, ProfileDto>();
            CreateMap<Friendship, FriendshipDto>();
        }
    }
}