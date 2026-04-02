using ChatService.Application.DTOs;
using MediatR;

namespace ChatService.Application.Features.Chats.Queries
{
    public record GetMessagesQuery(
        Guid ConversationId,
        int Limit = 50,
        DateTime? Before = null
    ) : IRequest<IEnumerable<MessageDto>>;
}