using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Response
{
    public class FriendshipDto
    {
        public Guid Id { get; set; }
        public Guid RequesterId { get; set; }
        public Guid ReceiverId { get; set; }
        public FriendshipStatus Status { get; set; }
    }
}