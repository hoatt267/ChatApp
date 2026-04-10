using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Application.Features.Chats.Queries.GetConversationById
{
    public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationDto>
    {
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IConversationEnricher _conversationEnricher;

        public GetConversationByIdQueryHandler(IRepository<Conversation> conversationRepository, IConversationEnricher conversationEnricher)
        {
            _conversationRepository = conversationRepository;
            _conversationEnricher = conversationEnricher;
        }

        public async Task<ConversationDto> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository.GetAsync<Conversation>(
                predicate: c => c.Id == request.ConversationId,
                include: q => q.Include(c => c.Participants)
            );

            if (conversation == null)
            {
                throw new NotFoundException(nameof(Conversation), request.ConversationId);
            }

            //  BẢO MẬT: Chặn không cho người ngoài phòng chọc API vào xem lén thông tin phòng
            if (!conversation.Participants.Any(p => p.UserId == request.UserId))
            {
                throw new ForbiddenException("Bạn không phải là thành viên của cuộc trò chuyện này.");
            }

            // 4. Gọi hàm gộp thông tin (Enrich) mà bạn đã viết sẵn từ trước để chuyển Entity -> DTO
            return await _conversationEnricher.EnrichSingleAsync(conversation);
        }
    }
}