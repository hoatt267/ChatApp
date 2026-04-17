namespace ChatService.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendFriendRequestReceivedAsync(Guid receiverId, Guid requesterId, string requesterName, string requesterAvatar);
        Task SendFriendRequestAcceptedAsync(Guid requesterId, Guid receiverId, string receiverName);
    }
}