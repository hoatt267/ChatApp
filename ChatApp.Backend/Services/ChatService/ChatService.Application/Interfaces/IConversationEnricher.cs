using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.Application.DTOs;
using ChatService.Domain.Entities;

namespace ChatService.Application.Interfaces
{
    public interface IConversationEnricher
    {
        // Hàm map cho một danh sách
        Task<IEnumerable<ConversationDto>> EnrichListAsync(IEnumerable<Conversation> conversations);

        // Hàm map cho 1 phòng chat duy nhất
        Task<ConversationDto> EnrichSingleAsync(Conversation conversation);

        Task<IEnumerable<MessageDto>> EnrichMessagesAsync(IEnumerable<Message> messages);
    }
}