using ChatApp.Shared.Interfaces;
using MediatR;
using UserService.Application.DTOs.Response;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Friends.Queries.GetFriends
{
    public class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, List<FriendProfileDto>>
    {
        private readonly IRepository<Friendship> _friendshipRepository;
        private readonly IRepository<UserProfile> _userProfileRepository;

        public GetFriendsQueryHandler(IRepository<Friendship> friendshipRepository, IRepository<UserProfile> userProfileRepository)
        {
            _friendshipRepository = friendshipRepository;
            _userProfileRepository = userProfileRepository;
        }

        public async Task<List<FriendProfileDto>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
        {
            var friendships = await _friendshipRepository.GetListAsync<Friendship>(
                predicate: f => f.Status == request.Status &&
                (
                    (request.Status == FriendshipStatus.Blocked && f.RequesterId == request.CurrentUserId) ||
                    (request.Status != FriendshipStatus.Blocked && (f.RequesterId == request.CurrentUserId || f.ReceiverId == request.CurrentUserId))
                )
    );

            if (!friendships.Any()) return new List<FriendProfileDto>();

            var partnerIds = friendships
                .Select(f => f.RequesterId == request.CurrentUserId ? f.ReceiverId : f.RequesterId)
                .Distinct()
                .ToList();

            var partnerProfiles = await _userProfileRepository.GetListAsync<UserProfile>(
                predicate: p => partnerIds.Contains(p.Id)
            );

            var profileById = partnerProfiles.ToDictionary(p => p.Id);

            var result = new List<FriendProfileDto>(friendships.Count());

            foreach (var friendship in friendships)
            {
                var targetUserId = friendship.RequesterId == request.CurrentUserId ? friendship.ReceiverId : friendship.RequesterId;

                if (profileById.TryGetValue(targetUserId, out var targetProfile))
                {
                    result.Add(new FriendProfileDto
                    {
                        FriendshipId = friendship.Id,
                        UserId = targetProfile.Id,
                        Email = targetProfile.Email,
                        FullName = targetProfile.FullName,
                        AvatarUrl = targetProfile.AvatarUrl,
                        Bio = targetProfile.Bio,
                        Status = friendship.Status.ToString(),
                        IsRequester = friendship.RequesterId == request.CurrentUserId
                    });
                }
            }

            return result.OrderBy(r => r.FullName).ToList();
        }
    }
}