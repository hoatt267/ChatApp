using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Wrappers;
using MediatR;
using UserService.Application.DTOs.Response;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Friends.Queries.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, PaginatedList<DiscoverUserDto>>
    {
        private sealed class FriendshipSnapshot
        {
            public Guid PartnerId { get; set; }
            public FriendshipStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private readonly IUserProfileRepository _profileRepository;
        private readonly IRepository<Friendship> _friendshipRepository;

        public SearchUsersQueryHandler(IUserProfileRepository profileRepository, IRepository<Friendship> friendshipRepository)
        {
            _profileRepository = profileRepository;
            _friendshipRepository = friendshipRepository;
        }

        public async Task<PaginatedList<DiscoverUserDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

            var keyword = request.Keyword?.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new PaginatedList<DiscoverUserDto>(new List<DiscoverUserDto>(), 0, pageNumber, pageSize);
            }

            var excludedUserIds = new List<Guid> { request.CurrentUserId };

            //search profile by keyword and exclude current user
            var result = await _profileRepository.SearchProfilesAsync(keyword, excludedUserIds, pageNumber, pageSize);

            if (result.Items.Count == 0)
                return new PaginatedList<DiscoverUserDto>(new List<DiscoverUserDto>(), result.Count, pageNumber, pageSize);

            //get list id of user in search result
            var targetUserIds = result.Items.Select(p => p.Id).ToList();

            //get list friendship between current user and user in search result
            var friendshipSnapshots = await _friendshipRepository.GetListAsync<FriendshipSnapshot>(
                selector: f => new FriendshipSnapshot
                {
                    PartnerId = f.RequesterId == request.CurrentUserId ? f.ReceiverId : f.RequesterId,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt
                },
                predicate: f => (f.RequesterId == request.CurrentUserId && targetUserIds.Contains(f.ReceiverId)) ||
                                (f.ReceiverId == request.CurrentUserId && targetUserIds.Contains(f.RequesterId)),
                disableTracking: true
            );

            //group by partner id and get latest friendship status for each partner
            var friendshipStatusByPartnerId = friendshipSnapshots
                .GroupBy(f => f.PartnerId)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(x => x.CreatedAt).First().Status
                );

            //map profile to DiscoverUserDto and set friendship status
            var dtos = new List<DiscoverUserDto>(result.Items.Count);

            //set friendship status for each user in search result
            foreach (var profile in result.Items)
            {
                var status = friendshipStatusByPartnerId.TryGetValue(profile.Id, out var friendshipStatus)
                    ? friendshipStatus
                    : FriendshipStatus.None;

                dtos.Add(new DiscoverUserDto
                {
                    UserId = profile.Id,
                    FullName = profile.FullName ?? string.Empty,
                    Email = profile.Email,
                    AvatarUrl = profile.AvatarUrl,
                    Bio = profile.Bio,
                    FriendshipStatus = status
                });
            }

            return new PaginatedList<DiscoverUserDto>(dtos, result.Count, pageNumber, pageSize);
        }
    }
}
