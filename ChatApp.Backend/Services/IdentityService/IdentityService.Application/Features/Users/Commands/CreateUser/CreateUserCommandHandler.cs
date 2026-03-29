using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using ChatApp.Shared.Exceptions;
using MediatR;
using ChatApp.Shared.Interfaces;

namespace IdentityService.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponseDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IMapper _mapper;

        public CreateUserCommandHandler(IRepository<User> userRepository, IRepository<Role> roleRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
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

            //Logic gán role mặc định cho user mới tạo
            var role = await _roleRepository.GetAsync<Role>(predicate: r => r.Name == RoleEnum.User);

            if (role == null)
            {
                role = new Role(RoleEnum.User);
                await _roleRepository.AddAsync(role);
            }

            user.UserRoles.Add(new UserRole(userId: user.Id, roleId: role.Id));

            await _userRepository.AddAsync(user);

            // Map sang DTO và trả về
            return _mapper.Map<UserResponseDto>(user);
        }
    }
}