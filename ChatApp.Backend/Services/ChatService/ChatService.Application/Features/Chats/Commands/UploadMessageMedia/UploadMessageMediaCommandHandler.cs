using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        public UploadMessageMediaCommandHandler(
            IMessageRepository messageRepository,
            IRepository<Conversation> conversationRepository,
            IBlobStorageService blobStorageService,
            IConversationEnricher enricher,
            IConfiguration configuration)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _blobStorageService = blobStorageService;
            _enricher = enricher;
            _configuration = configuration;
        }

        public async Task<MessageDto> Handle(UploadMessageMediaCommand request, CancellationToken cancellationToken)
        {
            // 1. Kiểm tra phòng chat và quyền truy cập (Chống Hacker)
            var conversation = await _conversationRepository.GetAsync<Conversation>(
                predicate: c => c.Id == request.ConversationId,
                include: q => q.Include(c => c.Participants)
            );

            if (conversation == null) throw new NotFoundException(nameof(Conversation), request.ConversationId);

            if (!conversation.Participants.Any(p => p.UserId == request.SenderId))
                throw new ForbiddenException("You are not a participant in this conversation.");

            // 2. Tự động nhận diện loại tin nhắn dựa trên ContentType
            var messageType = MessageType.Document; // Mặc định là Document
            if (request.ContentType.StartsWith("image/")) messageType = MessageType.Image;
            else if (request.ContentType.StartsWith("video/")) messageType = MessageType.Video;
            else if (request.ContentType.StartsWith("audio/")) messageType = MessageType.Audio;

            // 3. Ném file lên Azurite (Dùng xô chat-media đã cấu hình ở appsettings)
            var containerName = _configuration["AzureStorage:ChatMediaContainer"] ?? "chat-media";
            var fileUrl = await _blobStorageService.UploadFileAsync(request.FileStream, request.FileName, request.ContentType, containerName);

            var actualContent = !string.IsNullOrWhiteSpace(request.Content) ? request.Content : request.FileName;

            // 4. Lưu tin nhắn vào MongoDB
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

            // 5. Làm giàu dữ liệu (Lắp thêm Tên và Avatar người gửi) và trả về
            var enrichedMessages = await _enricher.EnrichMessagesAsync(new List<Message> { message });
            return enrichedMessages.First();
        }
    }
}