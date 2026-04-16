using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using MediatR;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Friends.Commands.BlockUser
{
    public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, bool>
    {
        private readonly IRepository<Friendship> _friendshipRepository;
        private readonly IRepository<UserProfile> _userProfileRepository;

        public BlockUserCommandHandler(IRepository<Friendship> friendshipRepository, IRepository<UserProfile> userProfileRepository)
        {
            _friendshipRepository = friendshipRepository;
            _userProfileRepository = userProfileRepository;
        }

        public async Task<bool> Handle(BlockUserCommand request, CancellationToken cancellationToken)
        {
            if (request.CurrentUserId == request.TargetUserId)
                throw new BadRequestException("You cannot block yourself.");

            var targetExists = await _userProfileRepository.ExistsAsync(u => u.Id == request.TargetUserId);
            if (!targetExists) throw new NotFoundException(nameof(UserProfile), request.TargetUserId);

            var friendship = await _friendshipRepository.GetAsync<Friendship>(
                predicate: f => (f.RequesterId == request.CurrentUserId && f.ReceiverId == request.TargetUserId) ||
                                (f.RequesterId == request.TargetUserId && f.ReceiverId == request.CurrentUserId)
            );

            if (friendship != null)
            {
                // Nếu đã có record, cập nhật trạng thái thành Blocked, 
                friendship.UpdateRequester(request.CurrentUserId);
                friendship.Block();
                await _friendshipRepository.SaveChangesAsync();
            }
            else
            {
                // Nếu 2 người chưa từng tương tác, tạo record mới và set Blocked
                var newFriendship = new Friendship(request.CurrentUserId, request.TargetUserId);
                newFriendship.Block();
                await _friendshipRepository.AddAsync(newFriendship);
            }

            return true;
        }
    }
}