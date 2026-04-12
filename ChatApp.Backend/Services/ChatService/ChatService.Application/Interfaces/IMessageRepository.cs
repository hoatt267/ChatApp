using ChatService.Domain.Entities;

namespace ChatService.Application.Interfaces
{
    public interface IMessageRepository
    {
        // Lưu 1 tin nhắn mới
        Task AddAsync(Message message);

        // Lấy lịch sử chat của 1 phòng (sắp xếp theo thời gian)
        Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId, int limit = 50, DateTime? before = null);

        // Đánh dấu đã đọc cho tất cả tin nhắn trong phòng
        Task MarkMessagesAsReadAsync(Guid conversationId, Guid userId, DateTime readAt);
    }
}