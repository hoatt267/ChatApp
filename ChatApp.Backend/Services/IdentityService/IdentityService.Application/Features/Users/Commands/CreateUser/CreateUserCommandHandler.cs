using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponseDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public CreateUserCommandHandler(IRepository<User> userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra xem email đã tồn tại chưa
            var existingUser = await _userRepository.ExistsAsync(u => u.Email == request.Email);
            if (existingUser)
            {
                throw new BadRequestException("Email is already in use.");
            }

            // Use BCrypt to hash the password before saving
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User(request.Email, hashedPassword, request.FullName, null);

            await _userRepository.AddAsync(user);

            // Map sang DTO và trả về
            return _mapper.Map<UserResponseDto>(user);
        }
    }
}