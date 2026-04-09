using ChatApp.Shared.Domain;

namespace ChatService.Domain.Entities
{
    public class Conversation : BaseEntity
    {
        private Conversation() { }

        public Conversation(string? title, bool isGroup)
        {
            Title = title;
            IsGroup = isGroup;
        }

        public string? Title { get; private set; }
        public bool IsGroup { get; private set; }

        public string? LastMessageContent { get; private set; }
        public Guid? LastMessageSenderId { get; private set; }
        public DateTime? LastMessageCreatedAt { get; private set; }

        // Navigation properties
        public ICollection<Participant> Participants { get; private set; } = new List<Participant>();

        // Behaviors
        public void UpdateTitle(string? title)
        {
            Title = title;
            UpdateTimestamp();
        }

        public void UpdateLastMessage(string content, Guid senderId, DateTime createdAt)
        {
            LastMessageContent = content;
            LastMessageSenderId = senderId;
            LastMessageCreatedAt = createdAt;

            UpdateTimestamp();
        }
    }
}