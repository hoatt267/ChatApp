using FluentValidation;

namespace UserService.Application.Features.Profiles.Commands.UploadAvatar
{
    public class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
    {
        public UploadAvatarCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.FileName).NotEmpty();
            RuleFor(x => x.ContentType).Must(c => c.StartsWith("image/")).WithMessage("Chỉ cho phép upload file ảnh.");
        }
    }
}