using System.Security.Claims;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Wrappers;
using ChatService.Application.DTOs;
using ChatService.Application.DTOs.Requests;
using ChatService.Application.Features.Chats.Commands.CreateGroupChat;
using ChatService.Application.Features.Chats.Commands.CreatePrivateChat;
using ChatService.Application.Features.Chats.Queries;
using ChatService.Application.Features.Chats.Queries.GetUserConversations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConversationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int limit = 50, [FromQuery] DateTime? before = null)
        {
            var query = new GetMessagesQuery(conversationId, limit, before);
            var resultDto = await _mediator.Send(query);

            var apiResponse = ApiResponse<IEnumerable<MessageDto>>.Ok(resultDto, "Messages retrieved successfully.");

            return Ok(apiResponse);
        }

        // API LẤY DANH SÁCH PHÒNG CHAT CỦA USER ĐANG ĐĂNG NHẬP
        [HttpGet]
        public async Task<IActionResult> GetUserConversations()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

            var query = new GetUserConversationsQuery(userId);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<ConversationDto>>.Ok(result, "Conversations retrieved successfully."));
        }

        // API TẠO HOẶC LẤY PHÒNG CHAT 1-1
        [HttpPost("private")]
        public async Task<IActionResult> CreateOrGetPrivateChat([FromBody] Guid targetUserId)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdString, out var currentUserId)) return Unauthorized();

            if (targetUserId == currentUserId)
                throw new BadRequestException("Cannot create a private chat with yourself.");

            var command = new CreatePrivateChatCommand(targetUserId, currentUserId);
            var result = await _mediator.Send(command);

            return Ok(ApiResponse<ConversationDto>.Ok(result, "Private chat created or retrieved successfully."));
        }

        // API TẠO PHÒNG CHAT NHÓM (GROUP)
        [HttpPost("group")]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatRequest request)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdString, out var currentUserId)) return Unauthorized();

            if (request.TargetUserIds == null || !request.TargetUserIds.Any())
                throw new BadRequestException("Target users cannot be empty.");

            var command = new CreateGroupChatCommand(request.Title, request.TargetUserIds, currentUserId);
            var result = await _mediator.Send(command);

            return Ok(ApiResponse<ConversationDto>.Ok(result, "Group chat created successfully."));
        }
    }
}