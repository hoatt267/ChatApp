using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using static ChatApp.Shared.Events.FriendshipEvents;

namespace ChatService.Application.EventConsumers
{
    public class UserUnblockedEventConsumer : IConsumer<UserUnblockedEvent>
    {
        private readonly IDistributedCache _cache;

        public UserUnblockedEventConsumer(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task Consume(ConsumeContext<UserUnblockedEvent> context)
        {
            var msg = context.Message;
            var key = $"block:{msg.UnblockerId}:{msg.UnblockedId}";

            // Xóa khóa khỏi Redis khi có lệnh Bỏ chặn
            await _cache.RemoveAsync(key);
        }
    }
}