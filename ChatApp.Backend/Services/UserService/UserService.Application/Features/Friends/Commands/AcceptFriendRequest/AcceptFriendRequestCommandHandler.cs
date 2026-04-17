using AutoMapper;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using MassTransit;
using MediatR;
using UserService.Application.DTOs.Response;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using static ChatApp.Shared.Events.FriendshipEvents;

namespace UserService.Application.Features.Friends.Commands.AcceptFriendRequest
{
    public class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand, FriendshipDto>
    {
        private readonly IRepository<Friendship> _friendshipRepository;
        private readonly IRepository<UserProfile> _profileRepository;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public AcceptFriendRequestCommandHandler(
            IRepository<Friendship> friendshipRepository,
            IRepository<UserProfile> profileRepository,
            IMapper mapper,
            IPublishEndpoint publishEndpoint)
        {
            _friendshipRepository = friendshipRepository;
            _profileRepository = profileRepository;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
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

            var receiverProfile = await _profileRepository.GetAsync<UserProfile>(
                predicate: p => p.Id == request.CurrentUserId
            );

            await _publishEndpoint.Publish(new FriendRequestAcceptedEvent
            {
                RequesterId = friendship.RequesterId,
                ReceiverId = request.CurrentUserId,
                ReceiverName = receiverProfile?.FullName ?? "Someone"
            }, cancellationToken);

            return _mapper.Map<FriendshipDto>(friendship);
        }
    }
}