using ChatApp.Shared.Events;
using ChatApp.Shared.Interfaces;
using ChatService.Domain.Entities;
using MassTransit;

namespace ChatService.Application.EventConsumers
{
    public class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly IRepository<User> _userRepository;

        public UserCreatedEventConsumer(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;

            // 1. Kiểm tra xem User này đã được lưu trước đó chưa (tránh lỗi duplicate)
            var exists = await _userRepository.ExistsAsync(u => u.Id == message.UserId);
            if (exists) return;

            // 2. Tạo bản sao User và lưu vào Database của ChatService
            var user = new User(message.UserId, message.Email, message.FullName);

            await _userRepository.AddAsync(user);
        }
    }
}