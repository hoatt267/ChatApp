using ChatService.Application.DTOs;
using MediatR;

namespace ChatService.Application.Features.Chats.Commands.CreateGroupChat
{
    public record CreateGroupChatCommand(string Title, List<Guid> TargetUserIds, Guid CurrentUserId) : IRequest<ConversationDto>;
}