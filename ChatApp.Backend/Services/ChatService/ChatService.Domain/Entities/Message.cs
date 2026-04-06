using ChatApp.Shared.Domain;
using ChatService.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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

        public Message(Guid conversationId, Guid senderId, string content, MessageType type = MessageType.Text, string? fileUrl = null, string? fileName = null)
        {
            ConversationId = conversationId;
            SenderId = senderId;
            Content = content;
            Type = type;
            FileUrl = fileUrl;
            FileName = fileName;
        }

        public Guid ConversationId { get; private set; }
        public Guid SenderId { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public HashSet<Guid> ReadBy { get; private set; } = new HashSet<Guid>();

        public string? FileUrl { get; private set; }
        [BsonRepresentation(BsonType.String)]
        public MessageType Type { get; private set; } = MessageType.Text;
        public string? FileName { get; private set; }

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