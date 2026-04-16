using MediatR;

namespace UserService.Application.Features.Friends.Commands.BlockUser
{
    public record BlockUserCommand(Guid CurrentUserId, Guid TargetUserId) : IRequest<bool>;
}