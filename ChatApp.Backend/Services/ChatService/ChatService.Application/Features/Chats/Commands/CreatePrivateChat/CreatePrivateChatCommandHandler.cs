using AutoMapper;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Application.DTOs.Responses;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Application.Features.Chats.Commands.CreatePrivateChat
{
    public class CreatePrivateChatCommandHandler : IRequestHandler<CreatePrivateChatCommand, ConversationDto>
    {
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IConversationEnricher _enricher;

        public CreatePrivateChatCommandHandler(IRepository<Conversation> conversationRepository, IConversationEnricher enricher)
        {
            _conversationRepository = conversationRepository;
            _enricher = enricher;
        }

        public async Task<ConversationDto> Handle(CreatePrivateChatCommand request, CancellationToken cancellationToken)
        {
            var existingChat = await _conversationRepository.GetAsync<Conversation>(
                predicate: c => !c.IsGroup &&
                                c.Participants.Any(p => p.UserId == request.CurrentUserId) &&
                                c.Participants.Any(p => p.UserId == request.TargetUserId),
                include: q => q.Include(c => c.Participants)
            );

            Conversation conversationToReturn = existingChat;

            // Nếu chưa có, tạo mới và lưu xuống DB
            if (conversationToReturn == null)
            {
                conversationToReturn = new Conversation(null, false);
                conversationToReturn.Participants.Add(new Participant(conversationToReturn.Id, request.CurrentUserId));
                conversationToReturn.Participants.Add(new Participant(conversationToReturn.Id, request.TargetUserId));
                await _conversationRepository.AddAsync(conversationToReturn);
            }

            return await _enricher.EnrichSingleAsync(conversationToReturn);
        }
    }
}