using AutoMapper;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Domain.Entities;
using MediatR;

namespace ChatService.Application.Features.Chats.Commands.CreateGroupChat
{
    public class CreateGroupChatCommandHandler : IRequestHandler<CreateGroupChatCommand, ConversationDto>
    {
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IMapper _mapper;

        public CreateGroupChatCommandHandler(IRepository<Conversation> conversationRepository, IMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<ConversationDto> Handle(CreateGroupChatCommand request, CancellationToken cancellationToken)
        {
            // 1. Lọc dữ liệu: Bỏ các ID trùng lặp và ID của chính mình (nếu Frontend lỡ gửi lên)
            var uniqueTargetIds = request.TargetUserIds
                .Where(id => id != request.CurrentUserId)
                .Distinct()
                .ToList();

            // 2. Validate nghiệp vụ
            if (uniqueTargetIds.Count < 2)
            {
                throw new BadRequestException("A group chat must have at least 2 other members (total 3 members).");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new BadRequestException("Group chat title is required.");
            }

            // 3. Tạo phòng chat (IsGroup = true)
            var newGroup = new Conversation(request.Title, true);

            // 4. Thêm chính mình vào nhóm (Là người tạo nhóm)
            newGroup.Participants.Add(new Participant(newGroup.Id, request.CurrentUserId));

            // 5. Thêm các thành viên khác
            foreach (var userId in uniqueTargetIds)
            {
                newGroup.Participants.Add(new Participant(newGroup.Id, userId));
            }

            // 6. Lưu xuống Database
            await _conversationRepository.AddAsync(newGroup);

            return _mapper.Map<ConversationDto>(newGroup);
        }
    }
}