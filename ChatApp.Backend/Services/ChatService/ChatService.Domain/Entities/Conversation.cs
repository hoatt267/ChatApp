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

        // Navigation properties
        public ICollection<Participant> Participants { get; private set; } = new List<Participant>();

        // Behaviors
        public void UpdateTitle(string? title)
        {
            Title = title;
            UpdateTimestamp();
        }
    }
}