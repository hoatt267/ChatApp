using IdentityService.Application.DTOs.Responses;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<LoginResponseDto>
    {
        public string Token { get; set; } = string.Empty;
    }
}