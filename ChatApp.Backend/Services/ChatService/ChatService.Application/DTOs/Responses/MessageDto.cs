namespace ChatService.Application.DTOs
{
    public record MessageDto(
        Guid Id,
        Guid SenderId,
        string SenderName,
        string SenderAvatarUrl,
        Guid ConversationId,
        string Content,
        DateTime CreatedAt,
        IEnumerable<Guid> ReadBy
    );
}