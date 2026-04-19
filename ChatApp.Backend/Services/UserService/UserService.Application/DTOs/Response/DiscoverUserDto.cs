using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Response
{
    public class DiscoverUserDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public FriendshipStatus FriendshipStatus { get; set; } = FriendshipStatus.None;
        // Sau này có thể thêm int MutualFriendsCount vào đây
    }
}