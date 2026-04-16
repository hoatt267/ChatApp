using System.Linq.Expressions;
using ChatApp.Shared.Wrappers;

namespace ChatApp.Shared.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<TResult?> GetAsync<TResult>(
        Expression<Func<T, TResult>>? selector = null,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        bool disableTracking = false,
        bool ignoreQueryFilters = false
        );

        Task<IEnumerable<TResult>> GetListAsync<TResult>(
            Expression<Func<T, TResult>>? selector = null,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false
        );

        Task<PaginatedList<TResult>> GetPaginatedListAsync<TResult>(
            int pageNumber,
            int pageSize,
            Expression<Func<T, TResult>>? selector = null,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false
        );

        Task<T> AddAsync(T entity);

        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        Task UpdateRangeAsync(IEnumerable<T> entities);

        Task SaveChangesAsync();

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}