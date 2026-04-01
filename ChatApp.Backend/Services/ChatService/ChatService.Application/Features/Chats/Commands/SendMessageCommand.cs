using ChatService.Application.DTOs;
using MediatR;

namespace ChatService.Application.Features.Chats.Commands
{
    public record SendMessageCommand(
        Guid ConversationId,
        Guid SenderId,
        string Content
    ) : IRequest<MessageDto>;
}