using MediatR;

namespace IdentityService.Application.Features.Users.Commands.SyncOldUsers
{
    public record SyncOldUsersCommand() : IRequest<int>;

}