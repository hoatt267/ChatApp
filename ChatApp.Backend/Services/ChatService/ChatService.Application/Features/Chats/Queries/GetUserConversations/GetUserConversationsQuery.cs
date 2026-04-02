using ChatService.Application.DTOs;
using MediatR;

namespace ChatService.Application.Features.Chats.Queries.GetUserConversations
{
    public record GetUserConversationsQuery(Guid UserId) : IRequest<IEnumerable<ConversationDto>>;
}