using System.Security.Claims;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Extensions;
using ChatApp.Shared.Wrappers;
using ChatService.API.Hubs;
using ChatService.Application.DTOs;
using ChatService.Application.DTOs.Requests;
using ChatService.Application.Features.Chats.Commands.CreateGroupChat;
using ChatService.Application.Features.Chats.Commands.CreatePrivateChat;
using ChatService.Application.Features.Chats.Commands.UploadMessageMedia;
using ChatService.Application.Features.Chats.Queries;
using ChatService.Application.Features.Chats.Queries.GetUserConversations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public ConversationsController(IMediator mediator, IHubContext<ChatHub> chatHubContext)
        {
            _mediator = mediator;
            _chatHubContext = chatHubContext;
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int limit = 50, [FromQuery] DateTime? before = null)
        {
            var currentUserId = User.GetUserId();
            var query = new GetMessagesQuery(conversationId, currentUserId, limit, before);
            var resultDto = await _mediator.Send(query);

            var apiResponse = ApiResponse<IEnumerable<MessageDto>>.Ok(resultDto, "Messages retrieved successfully.");

            return Ok(apiResponse);
        }

        [HttpPost("{conversationId}/messages/media")]
        public async Task<IActionResult> UploadMediaMessage([FromRoute] Guid conversationId, [FromForm] IFormFile file, [FromForm] string? content)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("No file uploaded.");

            if (file.Length > 25 * 1024 * 1024)
                throw new BadRequestException("File size exceeds 25MB limit.");

            var currentUserId = User.GetUserId();

            // Thực thi Command
            using var stream = file.OpenReadStream();
            var command = new UploadMessageMediaCommand(conversationId, currentUserId, stream, file.FileName, file.ContentType, content);

            var resultDto = await _mediator.Send(command);

            var apiResponse = ApiResponse<MessageDto>.Ok(resultDto, "Media message sent successfully.");

            await _chatHubContext.Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", apiResponse);

            return Ok(apiResponse);
        }

        // API LẤY DANH SÁCH PHÒNG CHAT CỦA USER ĐANG ĐĂNG NHẬP
        [HttpGet]
        public async Task<IActionResult> GetUserConversations()
        {
            var userId = User.GetUserId();

            var query = new GetUserConversationsQuery(userId);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<ConversationDto>>.Ok(result, "Conversations retrieved successfully."));
        }

        // API TẠO HOẶC LẤY PHÒNG CHAT 1-1
        [HttpPost("private")]
        public async Task<IActionResult> CreateOrGetPrivateChat([FromBody] Guid targetUserId)
        {
            var currentUserId = User.GetUserId();

            if (targetUserId == currentUserId)
                throw new BadRequestException("Cannot create a private chat with yourself.");

            var command = new CreatePrivateChatCommand(targetUserId, currentUserId);
            var result = await _mediator.Send(command);

            // Bắn tín hiệu bí mật gọi đích danh User 2 báo rằng "Ê, có người vừa tạo phòng với bạn kìa, reload danh bạ đi!"
            await _chatHubContext.Clients.User(targetUserId.ToString()).SendAsync("NewConversationCreated");

            return Ok(ApiResponse<ConversationDto>.Ok(result, "Private chat created or retrieved successfully."));
        }

        // API TẠO PHÒNG CHAT NHÓM (GROUP)
        [HttpPost("group")]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatRequest request)
        {
            var currentUserId = User.GetUserId();

            if (request.TargetUserIds == null || !request.TargetUserIds.Any())
                throw new BadRequestException("Target users cannot be empty.");

            var command = new CreateGroupChatCommand(request.Title, request.TargetUserIds, currentUserId);
            var result = await _mediator.Send(command);

            // Bắn tín hiệu bí mật gọi đích danh tất cả User trong TargetUserIds báo rằng "Ê, có người vừa tạo phòng với bạn kìa, reload danh bạ đi!"
            var targetUserIdsString = request.TargetUserIds.Select(id => id.ToString()).ToList();

            // Dùng Clients.Users để bắn tín hiệu đến tất cả thành viên được mời vào nhóm
            await _chatHubContext.Clients.Users(targetUserIdsString).SendAsync("NewConversationCreated");

            return Ok(ApiResponse<ConversationDto>.Ok(result, "Group chat created successfully."));
        }
    }
}