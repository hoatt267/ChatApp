using AutoMapper;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using MediatR;

namespace ChatService.Application.Features.Chats.Commands
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IConversationEnricher _enricher;

        public SendMessageCommandHandler(IMessageRepository messageRepository, IRepository<Conversation> conversationRepository, IConversationEnricher enricher)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _enricher = enricher;
        }

        public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository.GetAsync<Conversation>(predicate: c => c.Id == request.ConversationId);
            if (conversation == null)
            {
                throw new NotFoundException(nameof(Conversation), request.ConversationId);
            }

            var message = new Message(
                request.ConversationId,
                request.SenderId,
                request.Content,
                request.Type,
                request.FileUrl,
                request.FileName
            );
            await _messageRepository.AddAsync(message);

            var enrichedMessages = await _enricher.EnrichMessagesAsync(new List<Message> { message });
            return enrichedMessages.First();
        }
    }
}