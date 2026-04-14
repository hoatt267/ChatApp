using MediatR;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Profiles.Queries.GetProfile
{
    public record GetProfileQuery(Guid UserId) : IRequest<ProfileDto>;
}