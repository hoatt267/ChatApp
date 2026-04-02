using ChatService.Application.DTOs;
using MediatR;

namespace ChatService.Application.Features.Chats.Commands.CreatePrivateChat
{
    public record CreatePrivateChatCommand(Guid TargetUserId, Guid CurrentUserId) : IRequest<ConversationDto>;
}