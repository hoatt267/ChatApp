using MediatR;

namespace IdentityService.Application.Features.Users.Commands.UploadAvatar
{
    public record UploadAvatarCommand(
        Guid UserId,
        Stream FileStream,
        string FileName,
        string ContentType
    ) : IRequest<string>;
}