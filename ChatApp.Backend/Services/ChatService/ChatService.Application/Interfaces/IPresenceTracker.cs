namespace ChatService.Application.Interfaces
{
    public interface IPresenceTracker
    {
        // Trả về true nếu đây là kết nối đầu tiên (User vừa online)
        Task<bool> UserConnected(string userId, string connectionId);

        // Trả về true nếu đây là kết nối cuối cùng (User đã tắt hết thiết bị, thực sự offline)
        Task<bool> UserDisconnected(string userId, string connectionId);

        // Lấy danh sách ID các user đang online
        Task<string[]> GetOnlineUsers();
    }
}