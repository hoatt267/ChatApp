using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.Application.Interfaces;
using MediatR;

namespace ChatService.Application.Features.Chats.Commands.MarkAsRead
{
    public class MarkMessagesAsReadCommandHandler : IRequestHandler<MarkMessagesAsReadCommand, bool>
    {
        private readonly IMessageRepository _messageRepository;

        public MarkMessagesAsReadCommandHandler(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<bool> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
        {
            await _messageRepository.MarkMessagesAsReadAsync(request.ConversationId, request.UserId);
            return true;
        }
    }
}