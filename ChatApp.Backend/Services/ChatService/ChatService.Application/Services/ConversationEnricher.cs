using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Application.DTOs.Responses;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Domain.Enums;

namespace ChatService.Application.Services
{
    public class ConversationEnricher : IConversationEnricher
    {
        private readonly IRepository<User> _userRepository;

        public ConversationEnricher(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<ConversationDto>> EnrichListAsync(IEnumerable<Conversation> conversations)
        {
            var conversationList = conversations.ToList();
            if (!conversationList.Any()) return new List<ConversationDto>();

            // 1. Lấy tất cả User (Gom ID 1 lần duy nhất)
            var userIds = conversationList.SelectMany(c => c.Participants.Select(p => p.UserId)).ToHashSet();

            // Đảm bảo lấy luôn cả người gửi tin nhắn cuối (đề phòng họ đã thoát group)
            var senderIds = conversationList.Where(c => c.LastMessageSenderId.HasValue).Select(c => c.LastMessageSenderId!.Value);
            userIds.UnionWith(senderIds);

            var users = await _userRepository.GetListAsync<User>(predicate: u => userIds.Contains(u.Id));
            var userDictionary = users.ToDictionary(u => u.Id);

            return conversationList.Select(c =>
            {
                MessageDto? lastMessageDto = null;

                if (c.LastMessageSenderId.HasValue && c.LastMessageCreatedAt.HasValue)
                {
                    var senderFound = userDictionary.TryGetValue(c.LastMessageSenderId.Value, out var sender);

                    lastMessageDto = new MessageDto(
                        Guid.Empty,
                        c.LastMessageSenderId.Value,
                        senderFound ? sender!.FullName : "Người dùng ẩn danh",
                        senderFound ? (sender!.AvatarUrl ?? "") : "",
                        c.Id,
                        c.LastMessageContent ?? "",
                        c.LastMessageCreatedAt.Value,
                        c.LastMessageReadBy ?? new List<Guid>(),
                        MessageType.Text,
                        null,
                        null
                    );
                }

                return new ConversationDto(
                    c.Id,
                    c.Title,
                    c.IsGroup,
                    c.CreatedAt,
                    c.Participants.Select(p =>
                    {
                        var userFound = userDictionary.TryGetValue(p.UserId, out var user);
                        return new ParticipantDto(
                            p.UserId,
                            userFound ? user!.FullName : "Người dùng ẩn danh",
                            userFound ? (user!.AvatarUrl ?? "") : ""
                        );
                    }),
                    lastMessageDto
                );
            }).OrderByDescending(c => c.LastMessage?.CreatedAt ?? c.CreatedAt) // Sắp xếp theo tin nhắn cuối nếu có
              .ToList();
        }

        public async Task<ConversationDto> EnrichSingleAsync(Conversation conversation)
        {
            // Tái sử dụng hàm EnrichListAsync cho 1 phần tử
            var result = await EnrichListAsync(new List<Conversation> { conversation });
            return result.First();
        }

        public async Task<IEnumerable<MessageDto>> EnrichMessagesAsync(IEnumerable<Message> messages)
        {
            if (!messages.Any()) return new List<MessageDto>();

            // 1. Gom tất cả ID người gửi
            var senderIds = messages.Select(m => m.SenderId).ToHashSet();

            // 2. Query bảng User để lấy tên
            var users = await _userRepository.GetListAsync<User>(predicate: u => senderIds.Contains(u.Id));
            var userDictionary = users.ToDictionary(u => u.Id);

            // 3. Map bằng tay ra DTO
            return messages.Select(m =>
            {
                var userFound = userDictionary.TryGetValue(m.SenderId, out var user);
                return new MessageDto(
                    m.Id,
                    m.SenderId,
                    userFound ? user!.FullName : "Người dùng ẩn danh",
                    userFound ? (user!.AvatarUrl ?? "") : "",
                    m.ConversationId,
                    m.Content,
                    m.CreatedAt,
                    m.ReadBy,
                    m.Type,
                    m.FileUrl,
                    m.FileName
                );
            });
        }
    }
}