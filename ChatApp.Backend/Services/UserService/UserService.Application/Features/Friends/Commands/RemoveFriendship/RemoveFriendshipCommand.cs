using MediatR;

namespace UserService.Application.Features.Friends.Commands.RemoveFriendship
{
    public record RemoveFriendshipCommand(Guid CurrentUserId, Guid TargetUserId) : IRequest<bool>;
}