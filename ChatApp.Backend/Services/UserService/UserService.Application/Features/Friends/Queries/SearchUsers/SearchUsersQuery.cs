using ChatApp.Shared.Wrappers;
using MediatR;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Friends.Queries.SearchUsers
{
    public record SearchUsersQuery(Guid CurrentUserId, string Keyword, int PageNumber, int PageSize) : IRequest<PaginatedList<DiscoverUserDto>>;
}