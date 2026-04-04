using AutoMapper;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Application.Features.Chats.Queries
{
    public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, IEnumerable<MessageDto>>
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConversationEnricher _enricher;
        private readonly IRepository<Conversation> _conversationRepository;

        public GetMessagesQueryHandler(IMessageRepository messageRepository, IConversationEnricher enricher, IRepository<Conversation> conversationRepository)
        {
            _messageRepository = messageRepository;
            _enricher = enricher;
            _conversationRepository = conversationRepository;
        }

        public async Task<IEnumerable<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
        {
            // Kiểm tra xem người dùng có phải là thành viên của cuộc trò chuyện không
            var conversation = await _conversationRepository.GetAsync<Conversation>(
                predicate: c => c.Id == request.ConversationId,
                include: q => q.Include(c => c.Participants)
            );

            // Phòng chat không tồn tại
            if (conversation == null)
            {
                throw new NotFoundException(nameof(Conversation), request.ConversationId);
            }

            // Kiểm tra xem người dùng có phải là thành viên của cuộc trò chuyện không
            if (!conversation.Participants.Any(p => p.UserId == request.CurrentUserId))
            {
                throw new ForbiddenException("You are not a participant in this conversation.");
            }

            var messages = await _messageRepository.GetMessagesByConversationIdAsync(request.ConversationId, request.Limit, request.Before);

            // Đảo ngược thứ tự để trả về tin nhắn mới nhất trước
            messages.Reverse();

            return await _enricher.EnrichMessagesAsync(messages);
        }
    }
}