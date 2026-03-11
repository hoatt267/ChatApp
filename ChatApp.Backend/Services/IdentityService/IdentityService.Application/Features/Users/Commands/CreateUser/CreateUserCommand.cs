using IdentityService.Application.DTOs;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands
{
    public class CreateUserCommand : IRequest<UserResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }
}