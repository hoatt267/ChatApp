namespace ChatApp.Shared.Events
{
    public record UserUpdatedEvent
    {
        public Guid UserId { get; init; }
        public string FullName { get; init; } = null!;
        public string AvatarUrl { get; init; } = string.Empty;
    }
}