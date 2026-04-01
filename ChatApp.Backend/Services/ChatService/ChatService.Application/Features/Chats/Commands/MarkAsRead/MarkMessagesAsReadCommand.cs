using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace ChatService.Application.Features.Chats.Commands.MarkAsRead
{
    public record MarkMessagesAsReadCommand(Guid ConversationId, Guid UserId) : IRequest<bool>;
}