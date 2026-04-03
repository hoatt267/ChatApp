namespace ChatApp.Shared.Events
{
    public record UserCreatedEvent
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = null!;
        public string FullName { get; init; } = null!;
    }
}