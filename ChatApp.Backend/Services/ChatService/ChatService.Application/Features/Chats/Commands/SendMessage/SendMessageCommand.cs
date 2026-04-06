using ChatService.Application.DTOs;
using ChatService.Domain.Enums;
using MediatR;

namespace ChatService.Application.Features.Chats.Commands
{
    public record SendMessageCommand(
        Guid ConversationId,
        Guid SenderId,
        string Content,
        MessageType Type = MessageType.Text,
        string? FileUrl = null,
        string? FileName = null
    ) : IRequest<MessageDto>;
}