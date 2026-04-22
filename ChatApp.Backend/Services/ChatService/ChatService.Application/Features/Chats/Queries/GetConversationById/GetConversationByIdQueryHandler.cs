using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace ChatService.Application.Features.Chats.Queries.GetConversationById
{
    public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationDto>
    {
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IConversationEnricher _conversationEnricher;
        private readonly IDistributedCache _cache;

        public GetConversationByIdQueryHandler(IRepository<Conversation> conversationRepository, IConversationEnricher conversationEnricher, IDistributedCache cache)
        {
            _conversationRepository = conversationRepository;
            _conversationEnricher = conversationEnricher;
            _cache = cache;
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

            var conversationDto = await _conversationEnricher.EnrichSingleAsync(conversation);

            // Xu ly logic Redis
            if (!conversation.IsGroup && conversation.Participants.Count == 2)
            {
                var participantsList = conversation.Participants.ToList();
                var user1 = participantsList[0].UserId;
                var user2 = participantsList[1].UserId;

                // Check Redis cả 2 chiều: 1 chặn 2 HOẶC 2 chặn 1
                var isBlocked1 = await _cache.GetStringAsync($"block:{user1}:{user2}");
                var isBlocked2 = await _cache.GetStringAsync($"block:{user2}:{user1}");

                if (isBlocked1 != null || isBlocked2 != null)
                {
                    conversationDto = conversationDto with { IsBlocked = true };
                }
            }

            return conversationDto;
        }
    }
}