using AutoMapper;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Application.Features.Chats.Commands.CreatePrivateChat
{
    public class CreatePrivateChatCommandHandler : IRequestHandler<CreatePrivateChatCommand, ConversationDto>
    {
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IMapper _mapper;

        public CreatePrivateChatCommandHandler(IRepository<Conversation> conversationRepository, IMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<ConversationDto> Handle(CreatePrivateChatCommand request, CancellationToken cancellationToken)
        {
            var existingChat = await _conversationRepository.GetAsync<Conversation>(
                predicate: c => !c.IsGroup &&
                                c.Participants.Any(p => p.UserId == request.CurrentUserId) &&
                                c.Participants.Any(p => p.UserId == request.TargetUserId),
                include: q => q.Include(c => c.Participants)
            );

            if (existingChat != null)
            {
                return _mapper.Map<ConversationDto>(existingChat); // Đã có thì trả về
            }

            // 2. Nếu chưa có, tạo mới
            var newConversation = new Conversation(null, false);

            // Thêm 2 thành viên
            newConversation.Participants.Add(new Participant(newConversation.Id, request.CurrentUserId));
            newConversation.Participants.Add(new Participant(newConversation.Id, request.TargetUserId));

            // Lưu xuống DB bằng Generic Repo
            await _conversationRepository.AddAsync(newConversation);

            return _mapper.Map<ConversationDto>(newConversation);
        }
    }
}