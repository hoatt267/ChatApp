using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Application.DTOs.Responses;
using IdentityService.Application.Interfaces;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace IdentityService.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponseDto>
    {
        private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokenRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IMapper _mapper;

        public RefreshTokenCommandHandler(IRepository<Domain.Entities.RefreshToken> refreshTokenRepository, IJwtProvider jwtProvider, IMapper mapper)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtProvider = jwtProvider;
            _mapper = mapper;
        }

        public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Tìm refresh token trong database
            var refreshToken = await _refreshTokenRepository.GetAsync<Domain.Entities.RefreshToken>(
                predicate: rt => rt.Token == request.Token,
                include: q => q.Include(r => r.User)
                                .ThenInclude(u => u.UserRoles)
                                .ThenInclude(ur => ur.Role)
            );

            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new BadRequestException("Invalid or expired refresh token.");
            }

            // Đánh dấu refresh token cũ là đã sử dụng
            refreshToken.Revoke();

            // Tạo token JWT mới
            string newAccessToken = _jwtProvider.GenerateToken(refreshToken.User);

            // Tạo refresh token mới
            string newRefreshTokenString = _jwtProvider.GenerateRefreshToken();
            var newRefreshToken = new Domain.Entities.RefreshToken(newRefreshTokenString, DateTime.UtcNow.AddDays(7), refreshToken.UserId);

            await _refreshTokenRepository.AddAsync(newRefreshToken);

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString,
                User = _mapper.Map<UserResponseDto>(refreshToken.User)
            };
        }
    }
}