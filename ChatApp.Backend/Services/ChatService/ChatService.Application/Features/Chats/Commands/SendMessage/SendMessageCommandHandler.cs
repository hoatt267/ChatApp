using AutoMapper;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Protos;
using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace ChatService.Application.Features.Chats.Commands
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IConversationEnricher _enricher;
        private readonly IDistributedCache _cache;
        private readonly FriendshipGrpcService.FriendshipGrpcServiceClient _grpcClient;

        public SendMessageCommandHandler(
            IMessageRepository messageRepository,
            IRepository<Conversation> conversationRepository,
            IConversationEnricher enricher,
            IDistributedCache cache,
            FriendshipGrpcService.FriendshipGrpcServiceClient grpcClient)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _enricher = enricher;
            _cache = cache;
            _grpcClient = grpcClient;
        }

        public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository.GetAsync<Conversation>(
                predicate: c => c.Id == request.ConversationId,
                include: q => q.Include(c => c.Participants)
            );

            if (conversation == null)
                throw new NotFoundException(nameof(Conversation), request.ConversationId);

            // ==========================================
            // 🔒 BẢO MẬT: KIỂM TRA QUYỀN GỬI TIN NHẮN
            // ==========================================
            if (!conversation.IsGroup && conversation.Participants.Count == 2)
            {
                var otherUserId = conversation.Participants.First(p => p.UserId != request.SenderId).UserId;

                // 1. HỎI REDIS TRƯỚC
                var isBlocked1 = await _cache.GetStringAsync($"block:{request.SenderId}:{otherUserId}");
                var isBlocked2 = await _cache.GetStringAsync($"block:{otherUserId}:{request.SenderId}");

                if (isBlocked1 != null || isBlocked2 != null)
                {
                    throw new ForbiddenException("Không thể gửi tin nhắn vì một trong hai người đã chặn người kia.");
                }
                // 2. REDIS TRỐNG -> GỌI gRPC ĐỂ XÁC MINH LẠI LẦN CUỐI
                else
                {
                    bool isActuallyBlocked = false;
                    try
                    {
                        var grpcRequest = new CheckBlockRequest { UserId1 = request.SenderId.ToString(), UserId2 = otherUserId.ToString() };
                        var grpcResponse = await _grpcClient.CheckBlockStatusAsync(grpcRequest);
                        isActuallyBlocked = grpcResponse.IsBlocked;
                    }
                    catch (Exception)
                    {
                        isActuallyBlocked = false; // Fallback an toàn nếu gRPC lỗi
                    }

                    if (isActuallyBlocked)
                    {
                        // Khôi phục lại Redis để các request sau khỏi cần gọi gRPC
                        await _cache.SetStringAsync($"block:{request.SenderId}:{otherUserId}", "1");
                        throw new ForbiddenException("Không thể gửi tin nhắn vì một trong hai người đã chặn người kia.");
                    }
                }
            }

            var message = new Message(
                request.ConversationId,
                request.SenderId,
                request.Content,
                request.Type,
                request.FileUrl,
                request.FileName
            );
            await _messageRepository.AddAsync(message);

            var displayContent = request.Type == MessageType.Text ? request.Content : $"Đã gửi một tệp";
            conversation.UpdateLastMessage(displayContent, message.SenderId, message.CreatedAt);
            await _conversationRepository.SaveChangesAsync();

            var enrichedMessages = await _enricher.EnrichMessagesAsync(new List<Message> { message });
            return enrichedMessages.First();
        }
    }
}