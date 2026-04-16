using MediatR;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Friends.Commands.AcceptFriendRequest
{
    public record AcceptFriendRequestCommand(Guid FriendshipId, Guid CurrentUserId) : IRequest<FriendshipDto>;
}