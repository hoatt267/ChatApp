using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using MassTransit;
using MediatR;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using static ChatApp.Shared.Events.FriendshipEvents;

namespace UserService.Application.Features.Friends.Commands.RemoveFriendship
{
    public class RemoveFriendshipCommandHandler : IRequestHandler<RemoveFriendshipCommand, bool>
    {
        private readonly IRepository<Friendship> _friendshipRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        public RemoveFriendshipCommandHandler(IRepository<Friendship> friendshipRepository, IPublishEndpoint publishEndpoint)
        {
            _friendshipRepository = friendshipRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> Handle(RemoveFriendshipCommand request, CancellationToken cancellationToken)
        {
            var friendship = await _friendshipRepository.GetAsync<Friendship>(
                predicate: f => (f.RequesterId == request.CurrentUserId && f.ReceiverId == request.TargetUserId) ||
                                (f.RequesterId == request.TargetUserId && f.ReceiverId == request.CurrentUserId)
            );

            if (friendship == null)
                throw new NotFoundException("Friendship record not found");

            if (friendship.Status == FriendshipStatus.Blocked)
            {
                // Nếu trạng thái đang là Blocked, BẮT BUỘC người gọi API phải là người đã nhấn nút Chặn (Requester)
                if (friendship.RequesterId != request.CurrentUserId)
                {
                    throw new BadRequestException("Bạn không có quyền bỏ chặn người dùng này!");
                }
            }

            if (request.ActionType == FriendshipAction.Cancel || request.ActionType == FriendshipAction.Reject)
            {
                if (friendship.Status == FriendshipStatus.Accepted)
                    throw new BadRequestException("Người này đã trở thành bạn bè của bạn rồi. Không thể hủy lời mời!");
            }
            else if (request.ActionType == FriendshipAction.Unfriend)
            {
                if (friendship.Status == FriendshipStatus.Pending)
                    throw new BadRequestException("Hai bạn chưa phải là bạn bè!");
            }

            bool isUnblocking = friendship.Status == FriendshipStatus.Blocked;

            await _friendshipRepository.DeleteAsync(friendship);

            if (isUnblocking)
            {
                await _publishEndpoint.Publish(new UserUnblockedEvent
                {
                    UnblockerId = request.CurrentUserId,
                    UnblockedId = request.TargetUserId
                });
            }
            else
            {
                await _publishEndpoint.Publish(new FriendshipRemovedEvent
                {
                    ActorId = request.CurrentUserId,
                    TargetId = request.TargetUserId
                });
            }

            return true;
        }

    }
}