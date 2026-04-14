using ChatApp.Shared.Events;
using ChatApp.Shared.Interfaces;
using MassTransit;
using UserService.Domain.Entities;

namespace UserService.Application.EventConsumers
{
    public class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly IRepository<UserProfile> _userProfileRepository;

        public UserCreatedEventConsumer(IRepository<UserProfile> userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;

            // 1. Kiểm tra xem User này đã được lưu trước đó chưa (tránh lỗi duplicate)
            var exists = await _userProfileRepository.ExistsAsync(u => u.Id == message.UserId);
            if (exists) return;

            var userProfile = new UserProfile(
                id: message.UserId,
                email: message.Email,
                fullName: message.FullName
            );

            await _userProfileRepository.AddAsync(userProfile);
        }
    }
}