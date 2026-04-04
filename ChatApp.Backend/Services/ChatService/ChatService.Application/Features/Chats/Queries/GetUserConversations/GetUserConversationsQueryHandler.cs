using AutoMapper;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Application.DTOs.Responses;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Application.Features.Chats.Queries.GetUserConversations
{
    public class GetUserConversationsQueryHandler : IRequestHandler<GetUserConversationsQuery, IEnumerable<ConversationDto>>
    {
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IConversationEnricher _enricher;

        public GetUserConversationsQueryHandler(IRepository<Conversation> conversationRepository, IConversationEnricher enricher)
        {
            _conversationRepository = conversationRepository;
            _enricher = enricher;
        }

        public async Task<IEnumerable<ConversationDto>> Handle(GetUserConversationsQuery request, CancellationToken cancellationToken)
        {
            var conversations = await _conversationRepository.GetListAsync<Conversation>(
                predicate: c => c.Participants.Any(p => p.UserId == request.UserId),
                include: q => q.Include(c => c.Participants),
                orderBy: q => q.OrderByDescending(c => c.CreatedAt)
            );

            if (!conversations.Any())
                return Enumerable.Empty<ConversationDto>();

            return await _enricher.EnrichListAsync(conversations);
        }
    }
}