using IdentityService.Application.DTOs.Responses;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<LoginResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}