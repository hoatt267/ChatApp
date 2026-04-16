using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using MediatR;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Friends.Commands.RemoveFriendship
{
    public class RemoveFriendshipCommandHandler : IRequestHandler<RemoveFriendshipCommand, bool>
    {
        private readonly IRepository<Friendship> _friendshipRepository;
        public RemoveFriendshipCommandHandler(IRepository<Friendship> friendshipRepository)
        {
            _friendshipRepository = friendshipRepository;
        }

        public async Task<bool> Handle(RemoveFriendshipCommand request, CancellationToken cancellationToken)
        {
            var friendship = await _friendshipRepository.GetAsync<Friendship>(
                predicate: f => (f.RequesterId == request.CurrentUserId && f.ReceiverId == request.TargetUserId) ||
                                (f.RequesterId == request.TargetUserId && f.ReceiverId == request.CurrentUserId)
            );

            if (friendship == null)
                throw new NotFoundException("Friendship record not found");

            await _friendshipRepository.DeleteAsync(friendship);
            return true;
        }

    }
}