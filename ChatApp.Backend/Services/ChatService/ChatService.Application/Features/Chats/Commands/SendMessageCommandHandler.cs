using AutoMapper;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Domain.Entities;
using MediatR;

namespace ChatService.Application.Features.Chats.Commands
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
    {
        private readonly IRepository<Message> _messageRepository;
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IMapper _mapper;

        public SendMessageCommandHandler(IRepository<Message> messageRepository, IRepository<Conversation> conversationRepository, IMapper mapper)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository.GetAsync<Conversation>(predicate: c => c.Id == request.ConversationId);
            if (conversation == null)
            {
                throw new NotFoundException(nameof(Conversation), request.ConversationId);
            }

            var message = new Message(request.ConversationId, request.SenderId, request.Content);
            await _messageRepository.AddAsync(message);

            return _mapper.Map<MessageDto>(message);
        }
    }
}