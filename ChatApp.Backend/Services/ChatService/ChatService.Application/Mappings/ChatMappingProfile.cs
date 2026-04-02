using AutoMapper;
using ChatService.Application.DTOs;
using ChatService.Domain.Entities;

namespace ChatService.Application.Mappings
{
    public class ChatMappingProfile : Profile
    {
        public ChatMappingProfile()
        {
            CreateMap<Message, MessageDto>();

            CreateMap<Conversation, ConversationDto>();
        }
    }
}