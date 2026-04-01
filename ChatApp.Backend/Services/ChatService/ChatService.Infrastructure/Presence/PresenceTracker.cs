using System.Text.Json;
using ChatService.Application.Interfaces;
using StackExchange.Redis;

namespace ChatService.Infrastructure.Presence
{
    public class PresenceTracker : IPresenceTracker
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly string _presenceKey = "chat_presence";

        public PresenceTracker(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<bool> UserConnected(string userId, string connectionId)
        {
            var db = _redis.GetDatabase();
            bool isFirstConnection = false;
            var connectionsList = new List<string>();

            // Tìm xem user đã có kết nối nào trong Redis chưa
            var connectionsVal = await db.HashGetAsync(_presenceKey, userId);
            if (connectionsVal.HasValue)
            {
                connectionsList = JsonSerializer.Deserialize<List<string>>(connectionsVal!.ToString()) ?? new List<string>();
            }

            // Nếu đây là connectionId mới, thêm vào mảng
            if (!connectionsList.Contains(connectionId))
            {
                connectionsList.Add(connectionId);
                // Nếu mảng chỉ có đúng 1 phần tử -> User vừa từ Offline chuyển sang Online
                isFirstConnection = connectionsList.Count == 1;

                await db.HashSetAsync(_presenceKey, userId, JsonSerializer.Serialize(connectionsList));
            }

            return isFirstConnection;
        }

        public async Task<bool> UserDisconnected(string userId, string connectionId)
        {
            var db = _redis.GetDatabase();
            bool isOffline = false;

            var connectionsVal = await db.HashGetAsync(_presenceKey, userId);
            if (!connectionsVal.HasValue) return isOffline;

            var connectionsList = JsonSerializer.Deserialize<List<string>>(connectionsVal!.ToString()) ?? new List<string>();

            if (connectionsList.Contains(connectionId))
            {
                connectionsList.Remove(connectionId); // Rút connection hiện tại ra

                if (connectionsList.Count == 0)
                {
                    // Nếu không còn connection nào -> Thực sự Offline, xóa luôn ngăn kéo của User này
                    await db.HashDeleteAsync(_presenceKey, userId);
                    isOffline = true;
                }
                else
                {
                    // Vẫn còn thiết bị khác đang online -> Cập nhật lại mảng mới
                    await db.HashSetAsync(_presenceKey, userId, JsonSerializer.Serialize(connectionsList));
                }
            }

            return isOffline;
        }

        public async Task<string[]> GetOnlineUsers()
        {
            var db = _redis.GetDatabase();
            var users = await db.HashKeysAsync(_presenceKey);
            return users.Select(k => k.ToString()).ToArray();
        }
    }
}