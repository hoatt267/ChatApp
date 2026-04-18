using MediatR;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Friends.Commands.RemoveFriendship
{
    public record RemoveFriendshipCommand(Guid CurrentUserId, Guid TargetUserId, FriendshipAction ActionType) : IRequest<bool>;
}