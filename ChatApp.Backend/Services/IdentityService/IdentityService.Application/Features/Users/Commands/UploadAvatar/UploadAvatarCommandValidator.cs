using FluentValidation;

namespace IdentityService.Application.Features.Users.Commands.UploadAvatar
{
    public class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
    {
        public UploadAvatarCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.FileName).NotEmpty();
            RuleFor(x => x.ContentType).Must(c => c.StartsWith("image/")).WithMessage("Chỉ cho phép file ảnh.");
        }
    }
}