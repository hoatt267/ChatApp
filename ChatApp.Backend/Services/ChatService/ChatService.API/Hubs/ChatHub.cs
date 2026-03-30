using ChatApp.Shared.Wrappers;
using ChatService.Application.DTOs;
using ChatService.Application.Features.Chats.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatService.API.Hubs;

[Authorize] // Bắt buộc phải có JWT Token hợp lệ mới được kết nối
public class ChatHub : Hub
{
    private readonly IMediator _mediator;

    public ChatHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var senderId)) return;

        // 1. Gửi Command xuống Application
        var command = new SendMessageCommand(conversationId, senderId, content);

        // result lúc này là MessageDto thuần túy
        var resultDto = await _mediator.Send(command);

        // 2. Tầng API bọc dữ liệu vào ApiResponse chuẩn
        var apiResponse = ApiResponse<MessageDto>.Ok(resultDto, "Message sent successfully.");

        // 3. Gửi response chuẩn này tới tất cả mọi người trong phòng chat
        await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", apiResponse);
    }

    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }
}