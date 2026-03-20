using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Application.DTOs.Responses;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokenRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IMapper _mapper;

        public LoginCommandHandler(IRepository<User> userRepository, IRepository<Domain.Entities.RefreshToken> refreshTokenRepository, IJwtProvider jwtProvider, IMapper mapper)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtProvider = jwtProvider;
            _mapper = mapper;
        }

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Tìm user theo email
            var user = await _userRepository.GetAsync<User>(
                predicate: u => u.Email == request.Email,
                disableTracking: true
            );

            // Kiểm tra mật khẩu
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new BadRequestException("Invalid email or password.");
            }

            // Tạo token JWT
            string token = _jwtProvider.GenerateToken(user);

            // Tạo refresh token
            string refreshTokenString = _jwtProvider.GenerateRefreshToken();
            var refreshToken = new Domain.Entities.RefreshToken(refreshTokenString, DateTime.UtcNow.AddDays(7), user.Id);

            await _refreshTokenRepository.AddAsync(refreshToken);

            // Map user sang DTO
            var userDto = _mapper.Map<UserResponseDto>(user);

            return new LoginResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshTokenString,
                User = userDto
            };
        }
    }
}