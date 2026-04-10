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
        public List<Guid>? LastMessageReadBy { get; private set; } = new List<Guid>();

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
            LastMessageReadBy = new List<Guid> { senderId };

            UpdateTimestamp();
        }

        public void MarkLastMessageAsRead(Guid userId)
        {
            if (LastMessageReadBy == null) LastMessageReadBy = new List<Guid>();
            if (!LastMessageReadBy.Contains(userId))
            {
                LastMessageReadBy.Add(userId);
            }
        }
    }
}