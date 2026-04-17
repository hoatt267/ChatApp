namespace ChatApp.Shared.Events
{
    public class FriendshipEvents
    {
        public record FriendRequestSentEvent
        {
            public Guid RequesterId { get; init; }
            public Guid ReceiverId { get; init; }
            public string RequesterName { get; init; } = string.Empty;
            public string RequesterAvatar { get; init; } = string.Empty;
        }

        public record FriendRequestAcceptedEvent
        {
            public Guid RequesterId { get; init; }
            public Guid ReceiverId { get; init; }
            public string ReceiverName { get; init; } = string.Empty;
        }
    }
}