using ChatApp.Shared.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
    {
        private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokenRepository;

        public LogoutCommandHandler(IRepository<Domain.Entities.RefreshToken> refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var token = await _refreshTokenRepository.GetAsync<Domain.Entities.RefreshToken>(
                predicate: rt => rt.Token == request.RefreshToken
            );

            if (token != null && !token.IsRevoked)
            {
                token.Revoke();
                await _refreshTokenRepository.SaveChangesAsync();
            }
        }

    }
}