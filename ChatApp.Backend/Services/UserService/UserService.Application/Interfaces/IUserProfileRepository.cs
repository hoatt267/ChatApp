using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Wrappers;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces
{
    public interface IUserProfileRepository : IRepository<UserProfile>
    {
        Task<PaginatedList<UserProfile>> SearchProfilesAsync(string keyword, List<Guid> excludedUserIds, int pageNumber, int pageSize);
    }
}