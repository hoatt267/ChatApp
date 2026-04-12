using ChatService.Domain.Enums;
using ChatService.Domain.Models;

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
        IEnumerable<ReadReceipt> ReadBy,
        MessageType Type = MessageType.Text,
        string? FileUrl = null,
        string? FileName = null
    );
}