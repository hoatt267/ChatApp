using System.Linq.Expressions;
using IdentityService.Domain.Interfaces;
using IdentityService.Domain.Wrappers;
using IdentityService.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly IdentityDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public Repository(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public async Task<TResult?> GetAsync<TResult>(
        Expression<Func<T, TResult>>? selector = null,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        bool disableTracking = false
    )
        {
            IQueryable<T> queryable = _dbSet.AsQueryable();

            if (disableTracking)
            {
                queryable = queryable.AsNoTracking();
            }

            if (include != null)
            {
                queryable = include(queryable).AsSplitQuery();
            }

            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }

            if (orderBy != null)
            {
                queryable = orderBy(queryable);
            }

            if (selector != null)
            {
                return await queryable.Select(selector).FirstOrDefaultAsync().ConfigureAwait(false);
            }

            return await ((IQueryable<TResult>)queryable).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<TResult>> GetListAsync<TResult>(
            Expression<Func<T, TResult>>? selector = null,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
            bool disableTracking = false
        )
        {
            IQueryable<T> queryable = _dbSet.AsQueryable();

            if (disableTracking)
            {
                queryable = queryable.AsNoTracking();
            }

            if (include != null)
            {
                queryable = include(queryable);
            }

            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }

            if (orderBy != null)
            {
                queryable = orderBy(queryable);
            }

            if (selector != null)
            {
                return await queryable.Select(selector).ToListAsync().ConfigureAwait(false);
            }

            return await ((IQueryable<TResult>)queryable).ToListAsync().ConfigureAwait(false);
        }

        public async Task<PaginatedList<TResult>> GetPaginatedListAsync<TResult>(
            int pageNumber,
            int pageSize,
            Expression<Func<T, TResult>>? selector = null,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
            bool disableTracking = false
        )
        {
            IQueryable<T> queryable = _dbSet.AsQueryable();

            if (disableTracking)
            {
                queryable = queryable.AsNoTracking();
            }

            if (include != null)
            {
                queryable = include(queryable);
            }

            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }

            if (orderBy != null)
            {
                queryable = orderBy(queryable);
            }

            var count = await queryable.CountAsync().ConfigureAwait(false);

            // Get all
            if (pageSize == 0)
                pageSize = count;

            IQueryable<TResult> queryable2;
            List<TResult> items;
            if (selector != null)
            {
                queryable2 = queryable.Select(selector);
                items = await queryable2.Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync()
                                        .ConfigureAwait(false);
            }
            else
            {
                items = await ((IQueryable<TResult>)queryable).Skip((pageNumber - 1) * pageSize)
                                                              .Take(pageSize)
                                                              .ToListAsync()
                                                              .ConfigureAwait(false);
            }
            return new PaginatedList<TResult>(
                items: items,
                count: count,
                pageNumber: pageNumber,
                pageSize: pageSize);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity).ConfigureAwait(false);
            await SaveChangesAsync().ConfigureAwait(false);
            return entity;
        }

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("Entities collection cannot be null or empty.");

            await _dbSet.AddRangeAsync(entities).ConfigureAwait(false);
            await SaveChangesAsync().ConfigureAwait(false);

            return entities;
        }

        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
            await SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new DbUpdateException(
                    ex.Entries.FirstOrDefault()?.GetType().Name ?? string.Empty);
            }
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate).ConfigureAwait(false);
    }
}