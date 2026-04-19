using ChatApp.Shared.Wrappers;
using MediatR;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Friends.Queries.GetSuggestedUsers
{
    public record GetSuggestedUsersQuery(Guid CurrentUserId, int PageNumber, int PageSize = 10) : IRequest<PaginatedList<DiscoverUserDto>>;
}