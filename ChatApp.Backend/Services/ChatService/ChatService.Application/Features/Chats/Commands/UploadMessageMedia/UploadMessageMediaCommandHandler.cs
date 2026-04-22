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
using Microsoft.Extensions.Configuration;

namespace ChatService.Application.Features.Chats.Commands.UploadMessageMedia
{
    public class UploadMessageMediaCommandHandler : IRequestHandler<UploadMessageMediaCommand, MessageDto>
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConversationEnricher _enricher;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly FriendshipGrpcService.FriendshipGrpcServiceClient _grpcClient;

        public UploadMessageMediaCommandHandler(
            IMessageRepository messageRepository,
            IRepository<Conversation> conversationRepository,
            IBlobStorageService blobStorageService,
            IConversationEnricher enricher,
            IConfiguration configuration,
            IDistributedCache cache,
            FriendshipGrpcService.FriendshipGrpcServiceClient grpcClient)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _blobStorageService = blobStorageService;
            _enricher = enricher;
            _configuration = configuration;
            _cache = cache;
            _grpcClient = grpcClient;
        }

        public async Task<MessageDto> Handle(UploadMessageMediaCommand request, CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository.GetAsync<Conversation>(
                predicate: c => c.Id == request.ConversationId,
                include: q => q.Include(c => c.Participants)
            );

            if (conversation == null) throw new NotFoundException(nameof(Conversation), request.ConversationId);

            if (!conversation.Participants.Any(p => p.UserId == request.SenderId))
                throw new ForbiddenException("You are not a participant in this conversation.");

            // ==========================================
            // 🔒 BẢO MẬT: KIỂM TRA QUYỀN GỬI FILE
            // ==========================================
            if (!conversation.IsGroup && conversation.Participants.Count == 2)
            {
                var otherUserId = conversation.Participants.First(p => p.UserId != request.SenderId).UserId;

                var isBlocked1 = await _cache.GetStringAsync($"block:{request.SenderId}:{otherUserId}");
                var isBlocked2 = await _cache.GetStringAsync($"block:{otherUserId}:{request.SenderId}");

                if (isBlocked1 != null || isBlocked2 != null)
                {
                    throw new ForbiddenException("Không thể gửi tệp vì một trong hai người đã chặn người kia.");
                }
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
                        isActuallyBlocked = false;
                    }

                    if (isActuallyBlocked)
                    {
                        await _cache.SetStringAsync($"block:{request.SenderId}:{otherUserId}", "1");
                        throw new ForbiddenException("Không thể gửi tệp vì một trong hai người đã chặn người kia.");
                    }
                }
            }

            var messageType = MessageType.Document;
            if (request.ContentType.StartsWith("image/")) messageType = MessageType.Image;
            else if (request.ContentType.StartsWith("video/")) messageType = MessageType.Video;
            else if (request.ContentType.StartsWith("audio/")) messageType = MessageType.Audio;

            var containerName = _configuration["AzureStorage:ChatMediaContainer"] ?? "chat-media";
            var fileUrl = await _blobStorageService.UploadFileAsync(request.FileStream, request.FileName, request.ContentType, containerName);

            var actualContent = !string.IsNullOrWhiteSpace(request.Content) ? request.Content : request.FileName;

            var message = new Message(
                conversationId: request.ConversationId,
                senderId: request.SenderId,
                content: actualContent,
                type: messageType,
                fileUrl: fileUrl,
                fileName: request.FileName
            );

            await _messageRepository.AddAsync(message);

            string displayContent = !string.IsNullOrWhiteSpace(request.Content)
                ? request.Content
                : (messageType == MessageType.Image ? "[Hình ảnh]" :
                   messageType == MessageType.Video ? "[Video]" : "[Tệp đính kèm]");

            conversation.UpdateLastMessage(displayContent, message.SenderId, message.CreatedAt);
            await _conversationRepository.SaveChangesAsync();

            var enrichedMessages = await _enricher.EnrichMessagesAsync(new List<Message> { message });
            return enrichedMessages.First();
        }
    }
}