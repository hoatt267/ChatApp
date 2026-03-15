using FluentValidation;

namespace IdentityService.Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(p => p.Email).NotEmpty().EmailAddress().WithMessage("Invalid email format.");
            RuleFor(p => p.Password).NotEmpty().WithMessage("Password is required.");
        }
    }
}