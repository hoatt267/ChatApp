using ChatApp.Shared.Domain;
using ChatService.Domain.Enums;
using ChatService.Domain.Models;
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
        public List<ReadReceipt> ReadBy { get; private set; } = new List<ReadReceipt>();

        public string? FileUrl { get; private set; }
        [BsonRepresentation(BsonType.String)]
        public MessageType Type { get; private set; } = MessageType.Text;
        public string? FileName { get; private set; }

        // Behaviors
        public void MarkAsRead(Guid userId, DateTime readAt)
        {
            if (userId != SenderId && !ReadBy.Any(r => r.UserId == userId))
            {
                ReadBy.Add(new ReadReceipt { UserId = userId, ReadAt = readAt });
                UpdateTimestamp();
            }
        }
    }
}