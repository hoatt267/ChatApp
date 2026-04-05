using ChatApp.Shared.Events;
using ChatApp.Shared.Interfaces;
using ChatService.Domain.Entities;
using MassTransit;

namespace ChatService.Application.EventConsumers
{
    public class UserUpdatedEventConsumer : IConsumer<UserUpdatedEvent>
    {
        private readonly IRepository<User> _userRepository;

        public UserUpdatedEventConsumer(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
        {
            var message = context.Message;

            // 1. Tìm xem User này đã tồn tại trong DB của ChatService chưa
            var existingUser = await _userRepository.GetAsync<User>(predicate: u => u.Id == message.UserId);

            if (existingUser != null)
            {
                // 2. Nếu có rồi thì cập nhật thông tin mới nhất
                existingUser.UpdateProfile(message.FullName, message.AvatarUrl);

                await _userRepository.SaveChangesAsync();
            }
            else
            {
                // 3. (Phòng hờ) Nếu chưa có thì tạo mới luôn
                var newUser = new User(message.UserId, message.FullName, message.AvatarUrl);
                await _userRepository.AddAsync(newUser);
            }
        }
    }
}