using AutoMapper;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Application.Features.Chats.Queries.GetUserConversations
{
    public class GetUserConversationsQueryHandler : IRequestHandler<GetUserConversationsQuery, IEnumerable<ConversationDto>>
    {
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IMapper _mapper;

        public GetUserConversationsQueryHandler(IRepository<Conversation> conversationRepository, IMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ConversationDto>> Handle(GetUserConversationsQuery request, CancellationToken cancellationToken)
        {
            var conversations = await _conversationRepository.GetListAsync<Conversation>(
                predicate: c => c.Participants.Any(p => p.UserId == request.UserId),
                include: q => q.Include(c => c.Participants),
                orderBy: q => q.OrderByDescending(c => c.CreatedAt)
            );

            return _mapper.Map<IEnumerable<ConversationDto>>(conversations);
        }
    }
}