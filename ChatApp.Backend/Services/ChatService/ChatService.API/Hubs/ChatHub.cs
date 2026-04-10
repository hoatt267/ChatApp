using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Wrappers;
using ChatService.Application.DTOs;
using ChatService.Application.Features.Chats.Commands;
using ChatService.Application.Features.Chats.Commands.MarkAsRead;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using ChatApp.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ChatService.API.Hubs;

[Authorize] // Bắt buộc phải có JWT Token hợp lệ mới được kết nối
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IPresenceTracker _tracker;
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<ChatHub> _logger;
    private readonly IRepository<Conversation> _conversationRepository;

    public ChatHub(IMediator mediator, IPresenceTracker tracker, IRepository<User> userRepository, ILogger<ChatHub> logger, IRepository<Conversation> conversationRepository)
    {
        _mediator = mediator;
        _tracker = tracker;
        _userRepository = userRepository;
        _logger = logger;
        _conversationRepository = conversationRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.GetUserId();
        var userIdString = userId.ToString();

        var isFirstConnection = await _tracker.UserConnected(userIdString, Context.ConnectionId);

        // 2. Lấy tên của chính người vừa đăng nhập
        var me = await _userRepository.GetAsync<User>(predicate: u => u.Id == userId);
        var myName = me != null ? me.FullName : "Anonymous";
        var myAvatar = me != null ? (me.AvatarUrl ?? "") : "";

        if (isFirstConnection)
        {
            await Clients.Others.SendAsync("UserIsOnline", new { UserId = userIdString, FullName = myName, AvatarUrl = myAvatar });
        }

        var currentUsersOnline = await _tracker.GetOnlineUsers();

        // 3. Map danh sách ID đang online thành Tên thật
        var onlineGuidIds = currentUsersOnline
            .Where(id => Guid.TryParse(id, out _))
            .Select(Guid.Parse)
            .ToList();

        var onlineUsersDb = await _userRepository.GetListAsync<User>(predicate: u => onlineGuidIds.Contains(u.Id));

        var onlineUsersDto = onlineUsersDb.Select(u => new
        {
            UserId = u.Id.ToString(),
            FullName = u.FullName,
            AvatarUrl = u.AvatarUrl ?? "",
        }).ToList();

        await Clients.Caller.SendAsync("GetOnlineUsers", onlineUsersDto);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User!.GetUserId().ToString();
        // 1. Rút connection khỏi Redis
        var isOffline = await _tracker.UserDisconnected(userId, Context.ConnectionId);

        // 2. Nếu đã tắt hết tab/thiết bị -> Báo cho toàn hệ thống
        if (isOffline)
        {
            await Clients.Others.SendAsync("UserIsOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        var userId = Context.User!.GetUserId();

        // 1. Gửi Command xuống Application
        var command = new SendMessageCommand(conversationId, userId, content);

        try
        {
            var resultDto = await _mediator.Send(command);
            var apiResponse = ApiResponse<MessageDto>.Ok(resultDto, "Message sent successfully.");

            var conversation = await _conversationRepository.GetAsync<Conversation>(
                predicate: c => c.Id == conversationId,
                include: q => q.Include(c => c.Participants)
            );
            if (conversation != null)
            {
                var participantUserIds = conversation.Participants.Select(p => p.UserId.ToString()).ToList();

                await Clients.Users(participantUserIds).SendAsync("ReceiveMessage", apiResponse);
            }
        }
        catch (CustomValidationException ex)
        {
            var firstError = ex.Errors.Values.FirstOrDefault()?.FirstOrDefault() ?? "Invalid request data.";

            await Clients.Caller.SendAsync("ReceiveError", firstError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending message in Conversation {ConversationId} by User {UserId}", conversationId, userId);
            await Clients.Caller.SendAsync("ReceiveError", "An error occurred while sending the message.");
        }
    }

    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task NotifyTyping(Guid conversationId, bool isTyping)
    {
        var userId = Context.User!.GetUserId();

        // Bắn sự kiện "UserTyping" đến tất cả những người khác TRONG CÙNG PHÒNG CHAT
        await Clients.OthersInGroup(conversationId.ToString())
                     .SendAsync("UserTyping", conversationId, userId, isTyping);
    }

    public async Task MarkAsRead(Guid conversationId)
    {
        var userId = Context.User!.GetUserId();

        // 1. Gửi Command xuống MongoDB để lưu trạng thái
        var command = new MarkMessagesAsReadCommand(conversationId, userId);
        await _mediator.Send(command);

        // 2. Bắn loa thông báo cho những người khác TRONG CÙNG PHÒNG biết là anh này vừa xem tin nhắn
        await Clients.Group(conversationId.ToString())
                     .SendAsync("UserHasReadMessages", conversationId, userId);
    }
}