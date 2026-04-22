using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using static ChatApp.Shared.Events.FriendshipEvents;

namespace ChatService.Application.EventConsumers
{
    public class UserBlockedEventConsumer : IConsumer<UserBlockedEvent>
    {
        private readonly IDistributedCache _cache;

        public UserBlockedEventConsumer(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task Consume(ConsumeContext<UserBlockedEvent> context)
        {
            var msg = context.Message;
            var key = $"block:{msg.BlockerId}:{msg.BlockedId}";

            // Lưu vào Redis (Value = "1"). Không set hạn sử dụng để nó tồn tại đến khi Unblock
            await _cache.SetStringAsync(key, "1");
        }
    }
}