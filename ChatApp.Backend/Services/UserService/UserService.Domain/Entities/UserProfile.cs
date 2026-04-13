using ChatApp.Shared.Domain;

namespace UserService.Domain.Entities
{
    public class UserProfile : BaseEntity
    {
        public UserProfile(Guid id, string email, string? fullName, string? avatarUrl)
        {
            Id = id;
            Email = email;
            FullName = fullName;
            AvatarUrl = avatarUrl;
        }

        public string Email { get; private set; } = string.Empty;
        public string? FullName { get; private set; }
        public string? AvatarUrl { get; private set; }
        public string? Bio { get; private set; }

        public void UpdateProfile(string? fullName, string? avatarUrl, string? bio)
        {
            FullName = fullName ?? FullName;
            AvatarUrl = avatarUrl ?? AvatarUrl;
            Bio = bio ?? Bio;
            UpdateTimestamp();
        }
    }
}