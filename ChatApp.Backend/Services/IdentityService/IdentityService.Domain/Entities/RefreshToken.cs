namespace IdentityService.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public RefreshToken() { }
        public string Token { get; private set; } = null!;
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; } = false;

        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;

        public RefreshToken(string token, DateTime expiresAt, Guid userId)
        {
            Token = token;
            ExpiresAt = expiresAt;
            UserId = userId;
        }

        public void Revoke()
        {
            IsRevoked = true;
            UpdateTimestamp();
        }
    }
}