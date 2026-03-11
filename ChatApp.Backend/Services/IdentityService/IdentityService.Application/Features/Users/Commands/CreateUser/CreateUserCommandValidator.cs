using FluentValidation;

namespace IdentityService.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(p => p.ConfirmPassword)
            .Equal(p => p.Password).WithMessage("Confirm password does not match.");
    }
}