using ChatApp.Shared.Domain;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities
{
    public class Friendship : BaseEntity
    {
        private Friendship() { }

        public Friendship(Guid requesterId, Guid receiverId)
        {
            RequesterId = requesterId;
            ReceiverId = receiverId;
            Status = FriendshipStatus.Pending;
        }

        public Guid RequesterId { get; private set; }
        public Guid ReceiverId { get; private set; }
        public FriendshipStatus Status { get; private set; }

        public void Accept()
        {
            Status = FriendshipStatus.Accepted;
            UpdateTimestamp();
        }

        public void Block()
        {
            Status = FriendshipStatus.Blocked;
            UpdateTimestamp();
        }

        public void UpdateRequester(Guid currentUserId)
        {
            if (RequesterId == currentUserId)
                return;

            if (ReceiverId == currentUserId)
            {
                var temp = RequesterId;
                RequesterId = currentUserId;
                ReceiverId = temp;
                UpdateTimestamp();
                return;
            }

            throw new InvalidOperationException("Current user must be either the requester or receiver of the friendship.");
        }
    }
}