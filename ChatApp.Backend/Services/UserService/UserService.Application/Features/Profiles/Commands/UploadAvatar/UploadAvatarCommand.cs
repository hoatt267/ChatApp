using MediatR;

namespace UserService.Application.Features.Profiles.Commands.UploadAvatar
{
    public record UploadAvatarCommand(
        Guid UserId,
        Stream FileStream,
        string FileName,
        string ContentType
    ) : IRequest<string>;
}