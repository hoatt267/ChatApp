using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using ChatApp.Shared.Exceptions;
using MediatR;
using ChatApp.Shared.Interfaces;
using MassTransit;
using ChatApp.Shared.Events;

namespace IdentityService.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponseDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateUserCommandHandler(IRepository<User> userRepository, IRepository<Role> roleRepository, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
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
            var user = new User(request.Email, hashedPassword, request.FullName);

            //Logic gán role mặc định cho user mới tạo
            var role = await _roleRepository.GetAsync<Role>(predicate: r => r.Name == RoleEnum.User);

            if (role == null)
            {
                role = new Role(RoleEnum.User);
                await _roleRepository.AddAsync(role);
            }
            var userRole = new UserRole(userId: user.Id, roleId: role.Id);
            user.UserRoles.Add(userRole);

            await _userRepository.AddAsync(user);

            // Sau khi tạo user thành công, publish một sự kiện UserCreateEvent lên RabbitMQ
            var userCreateEvent = new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName ?? string.Empty,
            };

            await _publishEndpoint.Publish(userCreateEvent, cancellationToken);

            // Map sang DTO và trả về
            return _mapper.Map<UserResponseDto>(user);
        }
    }
}