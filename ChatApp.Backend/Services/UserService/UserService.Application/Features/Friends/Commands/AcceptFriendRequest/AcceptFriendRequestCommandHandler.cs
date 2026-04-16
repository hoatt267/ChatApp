using AutoMapper;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using MediatR;
using UserService.Application.DTOs.Response;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Friends.Commands.AcceptFriendRequest
{
    public class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand, FriendshipDto>
    {
        private readonly IRepository<Friendship> _friendshipRepository;
        private readonly IMapper _mapper;

        public AcceptFriendRequestCommandHandler(IRepository<Friendship> friendshipRepository, IMapper mapper)
        {
            _friendshipRepository = friendshipRepository;
            _mapper = mapper;
        }

        public async Task<FriendshipDto> Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
        {
            var friendship = await _friendshipRepository.GetAsync<Friendship>(
                predicate: f => f.Id == request.FriendshipId
            );

            if (friendship == null)
                throw new NotFoundException(nameof(Friendship), request.FriendshipId);

            if (friendship.ReceiverId != request.CurrentUserId)
                throw new ForbiddenException("You do not have permission to accept this friend request.");

            if (friendship.Status != FriendshipStatus.Pending)
                throw new BadRequestException($"Cannot accept request. Current status is {friendship.Status}.");

            friendship.Accept();
            await _friendshipRepository.SaveChangesAsync();

            return _mapper.Map<FriendshipDto>(friendship);
        }
    }
}