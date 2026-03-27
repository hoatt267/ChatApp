using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommand : IRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}