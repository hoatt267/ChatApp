using MediatR;
using UserService.Application.DTOs.Response;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Friends.Queries.GetFriends
{
    public record GetFriendsQuery(Guid CurrentUserId, FriendshipStatus Status) : IRequest<List<FriendProfileDto>>;
}