using ChatApp.Shared.Exceptions;
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