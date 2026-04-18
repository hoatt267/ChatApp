using ChatService.API.Hubs;
using ChatService.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.API.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRNotificationService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendFriendRequestReceivedAsync(Guid receiverId, Guid requesterId, string requesterName, string requesterAvatar)
        {
            await _hubContext.Clients.User(receiverId.ToString())
                .SendAsync("ReceiveFriendRequest", new
                {
                    UserId = requesterId,
                    FullName = requesterName,
                    AvatarUrl = requesterAvatar,
                    Message = $"{requesterName} sent you a friend request."
                });
        }

        public async Task SendFriendRequestAcceptedAsync(Guid requesterId, Guid receiverId, string receiverName)
        {
            await _hubContext.Clients.User(requesterId.ToString())
                .SendAsync("FriendRequestAccepted", new
                {
                    UserId = receiverId,
                    FullName = receiverName,
                    Message = $"{receiverName} accepted your friend request."
                });
        }

        public async Task SendFriendshipRemovedAsync(Guid targetId, Guid actorId)
        {
            await _hubContext.Clients.User(targetId.ToString())
                .SendAsync("FriendshipRemoved", new { UserId = actorId });
        }
    }
}