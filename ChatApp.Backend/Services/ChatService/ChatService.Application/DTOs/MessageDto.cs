namespace ChatService.Application.DTOs
{
    public record MessageDto(
        Guid Id,
        Guid SenderId,
        Guid ConversationId,
        string Content,
        DateTime CreatedAt
    );
}