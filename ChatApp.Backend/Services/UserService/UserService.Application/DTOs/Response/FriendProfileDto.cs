namespace UserService.Application.DTOs.Response
{
    public class FriendProfileDto
    {
        public Guid FriendshipId { get; set; }
        public Guid UserId { get; set; } // ID của người bạn
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Accepted, Blocked
        public bool IsRequester { get; set; }
    }
}