using ChatService.Application.DTOs.Responses;

namespace ChatService.Application.DTOs
{
    public record ConversationDto(
        Guid Id,
        string? Title,
        bool IsGroup,
        DateTime CreatedAt,
        IEnumerable<ParticipantDto> Participants,
        MessageDto? LastMessage = null
    );
}