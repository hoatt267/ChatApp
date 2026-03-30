using ChatApp.Shared.Domain;

namespace ChatService.Domain.Entities
{
    public class Participant : BaseEntity
    {
        private Participant() { }
        public Participant(Guid conversationId, Guid userId)
        {
            ConversationId = conversationId;
            UserId = userId;
        }

        public Guid ConversationId { get; private set; }
        public Guid UserId { get; private set; }

        // Navigation properties
        public Conversation Conversation { get; private set; } = null!;
    }
}