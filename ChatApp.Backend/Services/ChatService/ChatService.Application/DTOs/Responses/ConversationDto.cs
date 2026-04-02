namespace ChatService.Application.DTOs
{
    public record ConversationDto(Guid Id, string? Title, bool IsGroup, DateTime CreatedAt);
}