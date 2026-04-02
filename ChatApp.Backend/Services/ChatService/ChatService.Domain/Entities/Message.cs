using ChatApp.Shared.Domain;

namespace ChatService.Domain.Entities
{
    public class Message : BaseEntity
    {
        private Message() { }
        public Message(Guid conversationId, Guid senderId, string content)
        {
            ConversationId = conversationId;
            SenderId = senderId;
            Content = content;
        }

        public Guid ConversationId { get; private set; }
        public Guid SenderId { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public HashSet<Guid> ReadBy { get; private set; } = new HashSet<Guid>();

        // Behaviors
        public void MarkAsRead(Guid userId)
        {
            if (userId != SenderId && !ReadBy.Contains(userId))
            {
                ReadBy.Add(userId);
                UpdateTimestamp();
            }
        }
    }
}