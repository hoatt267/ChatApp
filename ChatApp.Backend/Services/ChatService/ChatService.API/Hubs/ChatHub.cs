using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Wrappers;
using ChatService.Application.DTOs;
using ChatService.Application.Features.Chats.Commands;
using ChatService.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatService.API.Hubs;

[Authorize] // Bắt buộc phải có JWT Token hợp lệ mới được kết nối
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IPresenceTracker _tracker;

    public ChatHub(IMediator mediator, IPresenceTracker tracker)
    {
        _mediator = mediator;
        _tracker = tracker;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            // 1. Lưu vào Redis
            var isFirstConnection = await _tracker.UserConnected(userId, Context.ConnectionId);

            // 2. Nếu là lần đầu bật app -> Báo cho toàn hệ thống
            if (isFirstConnection)
            {
                await Clients.Others.SendAsync("UserIsOnline", userId);
            }

            // 3. Trả về danh sách user đang online cho chính người vừa đăng nhập
            var currentUsersOnline = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsersOnline);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            // 1. Rút connection khỏi Redis
            var isOffline = await _tracker.UserDisconnected(userId, Context.ConnectionId);

            // 2. Nếu đã tắt hết tab/thiết bị -> Báo cho toàn hệ thống
            if (isOffline)
            {
                await Clients.Others.SendAsync("UserIsOffline", userId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var senderId)) return;

        // 1. Gửi Command xuống Application
        var command = new SendMessageCommand(conversationId, senderId, content);

        try
        {
            var resultDto = await _mediator.Send(command);
            var apiResponse = ApiResponse<MessageDto>.Ok(resultDto, "Message sent successfully.");
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", apiResponse);
        }
        catch (CustomValidationException ex)
        {
            var firstError = ex.Errors.Values.FirstOrDefault()?.FirstOrDefault() ?? "Invalid request data.";

            await Clients.Caller.SendAsync("ReceiveError", firstError);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", "An error occurred while sending the message.");
        }
    }

    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }
}