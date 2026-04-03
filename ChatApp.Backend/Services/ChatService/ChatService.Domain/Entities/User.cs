using ChatApp.Shared.Domain;

namespace ChatService.Domain.Entities
{
    // Bản sao của User từ IdentityService, phục vụ mục đích lưu trữ thông tin cơ bản của user trong ChatService mà không cần phải gọi API sang IdentityService mỗi lần cần hiển thị tên người gửi, v.v.
    public class User : BaseEntity
    {
        public string Email { get; private set; } = null!;
        public string FullName { get; private set; } = null!;

        public User() { }

        public User(Guid id, string email, string fullName)
        {
            Id = id;
            Email = email;
            FullName = fullName;
        }
    }
}