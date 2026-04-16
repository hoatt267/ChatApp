using MediatR;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Friends.Commands.SendFriendRequest
{
    public record SendFriendRequestCommand(Guid RequesterId, Guid ReceiverId) : IRequest<FriendshipDto>;
}