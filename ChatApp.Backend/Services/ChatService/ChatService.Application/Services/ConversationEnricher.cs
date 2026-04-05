using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Application.DTOs.Responses;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;

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
            if (!conversations.Any()) return new List<ConversationDto>();

            // Gom ID và lấy Tên
            var userIds = conversations.SelectMany(c => c.Participants.Select(p => p.UserId)).ToHashSet();
            var users = await _userRepository.GetListAsync<User>(predicate: u => userIds.Contains(u.Id));
            var userDictionary = users.ToDictionary(u => u.Id);

            // Map ra DTO
            return conversations.Select(c => new ConversationDto(
                c.Id,
                c.Title,
                c.IsGroup,
                c.CreatedAt,
                c.Participants.Select(p =>
                {
                    var userFound = userDictionary.TryGetValue(p.UserId, out var user);
                    return new ParticipantDto(
                        p.UserId,
                        userFound ? user!.FullName : "Anonymous User",
                        userFound ? (user!.AvatarUrl ?? "") : ""
                    );
                })
            ));
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
                    m.ReadBy
                );
            });
        }
    }
}