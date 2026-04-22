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

        // Event này dùng chung cho: Hủy kết bạn, Từ chối, Hủy lời mời
        public record FriendshipRemovedEvent
        {
            public Guid ActorId { get; init; }   // Người thực hiện nút bấm
            public Guid TargetId { get; init; }  // Người bị tác động
        }

        public record UserBlockedEvent
        {
            public Guid BlockerId { get; init; }
            public Guid BlockedId { get; init; }
        }

        public record UserUnblockedEvent
        {
            public Guid UnblockerId { get; init; }
            public Guid UnblockedId { get; init; }
        }
    }
}