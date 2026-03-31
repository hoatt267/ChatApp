using AutoMapper;
using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using MediatR;

namespace ChatService.Application.Features.Chats.Queries
{
    public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, IEnumerable<MessageDto>>
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public GetMessagesQueryHandler(IMessageRepository messageRepository, IMapper mapper)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
        {
            var messages = await _messageRepository.GetMessagesByConversationIdAsync(request.ConversationId, request.Limit, request.Before);

            // Đảo ngược thứ tự để trả về tin nhắn mới nhất trước
            messages.Reverse();

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }
    }
}