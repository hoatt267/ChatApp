using ChatApp.Shared.Repositories;
using ChatApp.Shared.Wrappers;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.DatabaseContext;

namespace UserService.Infrastructure.Repositories
{
    public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
    {
        private readonly UserDbContext _dbContext;

        public UserProfileRepository(UserDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaginatedList<UserProfile>> SearchProfilesAsync(string keyword, List<Guid> excludedUserIds, int pageNumber, int pageSize)
        {
            keyword = keyword.Trim();

            var query = _dbContext.UserProfiles
                .AsNoTracking()
                .Where(p => !excludedUserIds.Contains(p.Id))
                .Where(p => EF.Functions.ILike(p.FullName ?? string.Empty, $"%{keyword}%"));

            var count = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<UserProfile>(items, count, pageNumber, pageSize);
        }
    }
}