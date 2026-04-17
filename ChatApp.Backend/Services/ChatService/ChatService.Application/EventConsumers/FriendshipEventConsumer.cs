using ChatService.Application.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using static ChatApp.Shared.Events.FriendshipEvents;

namespace ChatService.Application.EventConsumers
{
    public class FriendshipEventConsumer :
        IConsumer<FriendRequestSentEvent>,
        IConsumer<FriendRequestAcceptedEvent>
    {
        private readonly INotificationService _notificationService;

        public FriendshipEventConsumer(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<FriendRequestSentEvent> context)
        {
            var msg = context.Message;
            await _notificationService.SendFriendRequestReceivedAsync(
                msg.ReceiverId,
                msg.RequesterId,
                msg.RequesterName,
                msg.RequesterAvatar);
        }

        public async Task Consume(ConsumeContext<FriendRequestAcceptedEvent> context)
        {
            var msg = context.Message;
            await _notificationService.SendFriendRequestAcceptedAsync(
                msg.RequesterId,
                msg.ReceiverId,
                msg.ReceiverName);
        }
    }
}