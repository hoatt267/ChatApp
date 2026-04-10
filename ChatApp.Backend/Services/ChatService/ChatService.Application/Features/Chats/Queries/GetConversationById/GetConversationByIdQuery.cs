using ChatService.Application.DTOs;
using MediatR;

namespace ChatService.Application.Features.Chats.Queries.GetConversationById
{
    public record GetConversationByIdQuery(Guid ConversationId, Guid UserId) : IRequest<ConversationDto>;
}