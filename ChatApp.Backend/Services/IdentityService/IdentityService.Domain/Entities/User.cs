using ChatApp.Shared.Domain;

namespace IdentityService.Domain.Entities
{
    public class User : BaseEntity
    {
        public User() { }
        public User(string email, string passwordHash, string? fullName = null)
        {
            Email = email;
            PasswordHash = passwordHash;
            FullName = fullName;
        }

        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string? FullName { get; private set; }

        public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

        public void AddRefreshToken(RefreshToken token)
        {
            RefreshTokens.Add(token);
        }

    }
}