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
    }
}