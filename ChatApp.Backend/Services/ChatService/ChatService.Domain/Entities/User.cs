using ChatApp.Shared.Domain;

namespace ChatService.Domain.Entities
{
    // Bản sao của User từ IdentityService, phục vụ mục đích lưu trữ thông tin cơ bản của user trong ChatService mà không cần phải gọi API sang IdentityService mỗi lần cần hiển thị tên người gửi, v.v.
    public class User : BaseEntity
    {
        public string FullName { get; private set; } = null!;
        public string? AvatarUrl { get; private set; }

        public User() { }

        public User(Guid id, string fullName)
        {
            Id = id;
            FullName = fullName;
        }

        public User(Guid id, string fullName, string avatarUrl)
        {
            Id = id;
            FullName = fullName;
            AvatarUrl = avatarUrl;
        }

        public void UpdateProfile(string fullName, string avatarUrl)
        {
            FullName = fullName;
            AvatarUrl = avatarUrl;
            UpdateTimestamp();
        }
    }
}