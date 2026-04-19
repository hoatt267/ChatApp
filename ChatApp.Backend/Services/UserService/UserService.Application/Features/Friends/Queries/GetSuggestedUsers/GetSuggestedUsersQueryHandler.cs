using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Wrappers;
using MediatR;
using UserService.Application.DTOs.Response;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Friends.Queries.GetSuggestedUsers
{
    public class GetSuggestedUsersQueryHandler : IRequestHandler<GetSuggestedUsersQuery, PaginatedList<DiscoverUserDto>>
    {
        private readonly IRepository<UserProfile> _profileRepository;
        private readonly IRepository<Friendship> _friendshipRepository;

        public GetSuggestedUsersQueryHandler(IRepository<UserProfile> profileRepository, IRepository<Friendship> friendshipRepository)
        {
            _profileRepository = profileRepository;
            _friendshipRepository = friendshipRepository;
        }

        public async Task<PaginatedList<DiscoverUserDto>> Handle(GetSuggestedUsersQuery request, CancellationToken cancellationToken)
        {
            var existingFriendships = await _friendshipRepository.GetListAsync<Friendship>(
                predicate: f => f.RequesterId == request.CurrentUserId || f.ReceiverId == request.CurrentUserId
            );

            var excludedUserIds = existingFriendships
                .Select(f => f.RequesterId == request.CurrentUserId ? f.ReceiverId : f.RequesterId)
                .ToList();
            excludedUserIds.Add(request.CurrentUserId);

            var pagedProfiles = await _profileRepository.GetPaginatedListAsync<UserProfile>(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                predicate: p => !excludedUserIds.Contains(p.Id),
                orderBy: q => q.OrderByDescending(p => p.CreatedAt)
            );

            var dtos = pagedProfiles.Items.Select(p => new DiscoverUserDto
            {
                UserId = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                AvatarUrl = p.AvatarUrl,
                Bio = p.Bio,
                FriendshipStatus = FriendshipStatus.None,
            }).ToList();

            return new PaginatedList<DiscoverUserDto>(dtos, pagedProfiles.Count, request.PageNumber, request.PageSize);
        }
    }
}